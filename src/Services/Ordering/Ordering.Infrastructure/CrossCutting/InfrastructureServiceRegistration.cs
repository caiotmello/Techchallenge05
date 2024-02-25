using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Contracts.Infrastructure;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Infrastructure.Email;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Repositories;

namespace Ordering.Infrastructure.CrossCutting
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var sqlServer = Environment.GetEnvironmentVariable("DB_HOST") ?? string.Empty;
            var sqlPort = Environment.GetEnvironmentVariable("DB_PORT") ?? string.Empty;
            var sqlUser = Environment.GetEnvironmentVariable("DB_USER") ?? string.Empty;
            var sqlPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;
            var sqlDatabase = Environment.GetEnvironmentVariable("DB_NAME") ?? string.Empty;

            var sqlConnectionString = $"Server={sqlServer};Database={sqlDatabase};TrustServerCertificate=True; User={sqlUser};Password={sqlPassword};";
            Console.WriteLine($"CAIO - {sqlConnectionString}");

            services.AddDbContext<OrderContext>(options =>
                options.UseSqlServer(sqlConnectionString));

            services.AddScoped<IOrderRepository, OrderRepository>();

            services.Configure<EmailSettings>(options =>
            {
                options.FromAddress = configuration.GetSection(nameof(EmailSettings))[nameof(EmailSettings.FromAddress)];
                options.ApiKey = configuration.GetSection(nameof(EmailSettings))[nameof(EmailSettings.ApiKey)];
                options.FromName = configuration.GetSection(nameof(EmailSettings))[nameof(EmailSettings.FromName)];

            });

            
            services.AddTransient<IEmailService, EmailService>();

            return services;

        }
    }
}
