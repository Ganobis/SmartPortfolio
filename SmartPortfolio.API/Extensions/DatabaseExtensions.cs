using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Infrastructure.Persistence;

namespace SmartPortfolio.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SmartPortfolioDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        return services;
    }

    public static WebApplication ApplyMigrations(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartPortfolioDbContext>();
            try
            {
                Console.WriteLine("--> Nakładanie migracji...");
                dbContext.Database.Migrate();
                Console.WriteLine("--> Migracje nałożone pomyślnie!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Błąd podczas migracji: {ex.Message}");
            }
        }
        return app;
    }
}