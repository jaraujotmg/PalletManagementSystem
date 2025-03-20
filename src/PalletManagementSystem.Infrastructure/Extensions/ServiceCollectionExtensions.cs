using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Services;
using PalletManagementSystem.Infrastructure.Data.Repositories;
using PalletManagementSystem.Infrastructure.Data;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Infrastructure.Services;
using PalletManagementSystem.Infrastructure.Services.SSRSIntegration;

namespace PalletManagementSystem.Infrastructure.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds infrastructure services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Existing infrastructure service registration
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("PalletManagementSystem.Infrastructure")));

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IPalletRepository, PalletRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Infrastructure Services
            services.AddScoped<IPrinterService, PrinterService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IUserPreferenceService, UserPreferenceService>();

            // Query Service
            services.AddScoped<IQueryService, QueryService>();

            // SSRS Integration
            services.AddHttpClient<ISSRSClient, SSRSClient>();
            services.AddScoped<IReportingService, ReportingService>();

            // Identity and Authentication
            services.AddScoped<WindowsAuthenticationService>();
            services.AddScoped<IUserContext, UserContext>();

            return services;
        }

        /// <summary>
        /// Adds core services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            // Domain Services
            services.AddScoped<IPalletService, PalletService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IPlatformValidationService, PlatformValidationService>();

            return services;
        }
    }
}