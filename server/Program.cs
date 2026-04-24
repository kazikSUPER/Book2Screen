// <copyright file="Program.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text;
using AutoMapper;
using Book2Screen.API__Web_.Middleware;
using Book2Screen.Application.Interfaces;
using Book2Screen.Application.Mappings;
using Book2Screen.Application.Services;
using Book2Screen.Application.Validators;
using Book2Screen.Infrastructure.ExternalServices;
using Book2Screen.Infrastructure.Persistence;
using Book2Screen.Infrastructure.Persistence.Seed;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Extensions.Logging;

if (File.Exists("../.env"))
{
    DotNetEnv.Env.Load("../.env");
}
else
{
    DotNetEnv.Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

var mappingConfig = new MapperConfiguration(
mc =>
{
    mc.AddProfile(new AdaptationProfile());
},
new SerilogLoggerFactory(Log.Logger));

IMapper mapper = mappingConfig.CreateMapper();

builder.Services.AddSingleton(mapper);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdaptationService, AdaptationService>();
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret ?? throw new InvalidOperationException("JWT secret not found."))),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddFluentValidationRulesToSwagger();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введіть JWT токен у форматі: Bearer {ваш_токен}",
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = [],
    });

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Book2Screen API", Version = "v1" });
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddNpgSql(
    connectionString: connectionString ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
    name: "Supabase_PostgreSQL",
    tags: new[] { "db", "sql", "supabase" },
    failureStatus: HealthStatus.Unhealthy,
    timeout: TimeSpan.FromSeconds(5))
    .AddDbContextCheck<ApplicationDbContext>(
    name: "EF_Core_Context",
    tags: new[] { "orm", "efcore" });

var app = builder.Build();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<ApplicationDbContext>();

    var retryCount = 10;
    while (retryCount > 0)
    {
        try
        {
            logger.LogInformation("Applying migrations and seeding (Attempts left: {Count})", retryCount);

            try
            {
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            catch (Exception migrateEx)
            {
                logger.LogWarning(migrateEx, "Migration skipped - database may already be up to date. Message: {Message}", migrateEx.Message);
            }

            try
            {
                await DbSeeder.SeedAsync(db);
                logger.LogInformation("Database seeding completed.");
            }
            catch (Exception seedEx)
            {
                logger.LogWarning(seedEx, "Database seeding skipped - data may already exist. Message: {Message}", seedEx.Message);
            }

            logger.LogInformation("Database is ready.");
            break;
        }
        catch (Exception ex)
        {
            retryCount--;
            logger.LogError(ex, "Error during database initialization. Message: {Message}", ex.Message);
            if (retryCount == 0)
            {
                logger.LogCritical(ex, "Database connection failed permanently.");
                throw;
            }

            logger.LogWarning("Waiting for database... (5s)");
            await Task.Delay(5000);
        }
    }
}

try
{
    Log.Information("Starting web application");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
