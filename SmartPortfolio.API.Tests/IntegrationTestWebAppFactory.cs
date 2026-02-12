using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartPortfolio.Domain.Interfaces;
using SmartPortfolio.Infrastructure.Persistence;
using SmartPortfolio.Infrastructure.Services;
using Testcontainers.MsSql;

namespace SmartPortfolio.API.Tests
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SmartPortfolioDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<SmartPortfolioDbContext>(options =>
                {
                    options.UseSqlServer(_dbContainer.GetConnectionString());
                });

                services.RemoveAll<ICurrencyConverter>();

                services.AddSingleton<ICurrencyConverter, FakeCurrencyConverter>();
            });
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            var options = new DbContextOptionsBuilder<SmartPortfolioDbContext>()
                .UseSqlServer(_dbContainer.GetConnectionString())
                .Options;

            using var context = new SmartPortfolioDbContext(options);
            await context.Database.EnsureCreatedAsync();
        }
        public new Task DisposeAsync() => _dbContainer.StopAsync();
    }
}