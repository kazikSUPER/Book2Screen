using Book2Screen.Application.Interfaces;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Book2Screen.Test.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Видаляємо всі реєстрації, пов'язані з DbContext та його опціями
            var dbDescriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                     d.ServiceType == typeof(ApplicationDbContext) ||
                     d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();

            foreach (var descriptor in dbDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
                options.ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // Замінюємо IEmailService на мок, щоб не відправляти реальні листи
            var emailDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmailService));
            if (emailDescriptor != null)
            {
                services.Remove(emailDescriptor);
            }

            var emailServiceMock = new Mock<IEmailService>();
            services.AddScoped(_ => emailServiceMock.Object);

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

                db.Database.EnsureCreated();
                Book2Screen.Infrastructure.Persistence.Seed.DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
            }
        });

        // Забезпечуємо наявність необхідних змінних середовища для тестів
        Environment.SetEnvironmentVariable("JWT_SECRET", "super_secret_key_for_testing_purposes_only_12345");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "Book2ScreenTests");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "Book2ScreenTests");
        Environment.SetEnvironmentVariable("JWT_EXPIRY_MINUTES", "60");
        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", "Host=localhost;Database=test;Username=test;Password=test");
    }
}
