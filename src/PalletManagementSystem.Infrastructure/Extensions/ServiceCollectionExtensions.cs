using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Services;
using PalletManagementSystem.Infrastructure.Data;
using PalletManagementSystem.Infrastructure.Data.Repositories;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Infrastructure.Logging;
using PalletManagementSystem.Infrastructure.Services;
using PalletManagementSystem.Infrastructure.Services.SSRSIntegration;
using System;
using System.Net.Http;

namespace PalletManagementSystem.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds infrastructure services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add HttpContextAccessor
            services.AddHttpContextAccessor();

            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Register repositories
            services.AddScoped<IPalletRepository, PalletRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();

            // Register domain services
            services.AddScoped<PalletNumberGenerator>();

            // Register infrastructure services
            services.AddHttpClient<SSRSClient>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<IPrinterService, PrinterService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IUserPreferenceService, UserPreferenceService>();

            // Register identity services
            services.AddScoped<WindowsAuthenticationService>();
            services.AddScoped<UserContext>();

            // Register logging service
            services.AddScoped<LoggingService>();

            // Register application services
            services.AddScoped<IPalletService, PalletService>();
            services.AddScoped<IItemService, ItemService>();

            return services;
        }
    }