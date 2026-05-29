// <copyright file="Program.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using AutoMapper;
using Book2Screen.API__Web_.Configurations;
using Book2Screen.API__Web_.Middleware;
using Book2Screen.Application.Interfaces;
using Book2Screen.Application.Mappings;
using Book2Screen.Application.Services;
using Book2Screen.Application.Validators;
using Book2Screen.Infrastructure.ExternalServices;
using Book2Screen.Infrastructure.Persistence;
using Book2Screen.Infrastructure.Persistence.Seed;
using FluentValidation;
using HealthChecks.UI.Client;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
var jwtOptions = new JwtOptions
{
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
    ExpiryMinutes = Convert.ToInt32(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")),
    Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT secret not found."),
};

builder.Services.AddSingleton(jwtOptions);

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

builder.Services.Configure<EmailOptions>(options =>
{
    options.SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "smtp.gmail.com";
    options.SmtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : 587;
    options.SenderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL") ?? string.Empty;
    options.SenderPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD") ?? string.Empty;
    options.SenderName = Environment.GetEnvironmentVariable("SENDER_NAME") ?? "Book2Screen";
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAdaptationService, AdaptationService>();
builder.Services.AddScoped<IWorkService, WorkService>();
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();

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
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret ?? throw new InvalidOperationException("JWT secret not found."))),
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

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Book2Screen API", Version = "v1" });
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
            }));
});

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

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging();

app.MapControllers();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

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

/// <summary>
/// Partial class Program for integration tests.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Program"/> class.
    /// </summary>
    protected Program()
    {
    }
}
