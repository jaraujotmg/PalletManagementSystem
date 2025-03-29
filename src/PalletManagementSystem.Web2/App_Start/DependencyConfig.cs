// src/PalletManagementSystem.Web2/App_Start/DependencyConfig.cs
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Infrastructure.Data;
using PalletManagementSystem.Infrastructure.Data.Repositories;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Infrastructure.Services;
//using PalletManagementSystem.Infrastructure.Services.SSRSIntegration;
using PalletManagementSystem.Web2.Services;

namespace PalletManagementSystem.Web2.App_Start
{
    /// <summary>
    /// Manages dependency registration for the application
    /// </summary>
    public class DependencyConfig
    {
        /// <summary>
        /// Registers all dependencies and returns the configured container
        /// </summary>
        public static IContainer RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // Register configuration wrapper
            var appSettings = new AppSettingsWrapper();
            builder.RegisterInstance(appSettings).As<IAppSettings>().SingleInstance();

            // --- Register Logging ---

            // 1. Ensure you have the necessary NuGet packages installed, e.g.,
            //    Install-Package Microsoft.Extensions.Logging
            //    Install-Package Microsoft.Extensions.Logging.Debug
            //    Install-Package Microsoft.Extensions.Logging.Console (if using Console)

            // 2. Create a LoggerFactory instance directly (suitable for .NET Framework)
            var loggerFactory = new LoggerFactory();

            // 3. Add providers using extension methods.
            //    The specific methods might vary slightly based on provider package versions.
            //    You might need to specify a minimum LogLevel here.
            loggerFactory.AddDebug(LogLevel.Trace); // Add Debug provider (logs to Output window)

            // Example: Add Console provider (ensure Microsoft.Extensions.Logging.Console is installed)
            // loggerFactory.AddConsole(LogLevel.Information);

            // Add other providers as needed (e.g., file logging)

            // 4. Register the configured factory as a singleton
            builder.RegisterInstance<ILoggerFactory>(loggerFactory).SingleInstance();

            // 5. Register a way to create generic ILogger<T> instances (this remains the same)
            //    Autofac will use the registered ILoggerFactory to create these
            builder.RegisterGeneric(typeof(Logger<>))
                   .As(typeof(ILogger<>))
                   .InstancePerDependency(); // Loggers are typically lightweight

            // --- End Logging Registration ---

            // Register components from each project layer
            builder.RegisterCoreComponents();
            builder.RegisterInfrastructureComponents(appSettings); // Pass builder here if needed, but logging is already registered
            builder.RegisterWebComponents();

            // Important: Set the dependency resolver for MVC
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container)); // Make sure this line exists in your Global.asax Application_Start or here

            return container;
        }
    }

    /// <summary>
    /// Extension methods for registering components from different project layers
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers components from the Core project
        /// </summary>
        public static void RegisterCoreComponents(this ContainerBuilder builder)
        {
            // Core services have per-request lifetime as they might contain
            // request-specific logic or data

            // Register domain services
            builder.RegisterType<Core.Services.PalletService>()
                .As<IPalletService>()
                .InstancePerRequest()
                .PropertiesAutowired();

            builder.RegisterType<Core.Services.ItemService>()
                .As<IItemService>()
                .InstancePerRequest()
                .PropertiesAutowired();

            builder.RegisterType<Core.Services.PlatformValidationService>()
                .As<IPlatformValidationService>()
                .InstancePerRequest()
                .PropertiesAutowired();

            builder.RegisterType<Core.Services.PalletNumberGenerator>()
                .As<IPalletNumberGenerator>()
                .InstancePerRequest()
                .PropertiesAutowired();
        }

        /// <summary>
        /// Registers components from the Infrastructure project
        /// </summary>
        public static void RegisterInfrastructureComponents(this ContainerBuilder builder, IAppSettings appSettings)
        {
            // Register DbContext
            builder.Register(c => {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                var connectionString = appSettings.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
                // Optional: Inject ILoggerFactory into DbContext if needed for EF Core logging
                // var loggerFactory = c.Resolve<ILoggerFactory>();
                // optionsBuilder.UseLoggerFactory(loggerFactory);
                return new ApplicationDbContext(optionsBuilder.Options);
            })
                       .InstancePerRequest() // Database context should be scoped to the request
            .PropertiesAutowired();

            // Data access components - per request lifetime to ensure
            // consistent data access within a request
            RegisterDataAccessComponents(builder);

            // External service integrations - per request lifetime as they
            // may contain request-specific configuration
            RegisterExternalServiceComponents(builder, appSettings);

            // Identity services - per request lifetime as they contain
            // user-specific information
            RegisterIdentityComponents(builder);
        }

        /// <summary>
        /// Registers data access components from the Infrastructure project
        /// </summary>
        private static void RegisterDataAccessComponents(ContainerBuilder builder)
        {
            // Generic repositories
            builder.RegisterGeneric(typeof(Repository<>))
                .As(typeof(IRepository<>))
                .InstancePerRequest();

            // Specific repositories
            builder.RegisterType<PalletRepository>()
                .As<IPalletRepository>()
                .InstancePerRequest()
                .PropertiesAutowired();

            builder.RegisterType<ItemRepository>()
                .As<IItemRepository>()
                .InstancePerRequest()
                .PropertiesAutowired();

            // Unit of work - must be per request to maintain transaction integrity
            builder.RegisterType<UnitOfWork>()
                .As<IUnitOfWork>()
                .InstancePerRequest()
                .PropertiesAutowired();

            // Transaction manager
            builder.RegisterType<TransactionManager>()
                .As<ITransactionManager>()
                .InstancePerRequest()
                .PropertiesAutowired();
        }

        /// <summary>
        /// Registers external service integrations from the Infrastructure project
        /// </summary>
        private static void RegisterExternalServiceComponents(ContainerBuilder builder, IAppSettings appSettings)
        {
            //// Printer service - per request as it may use request-specific settings
            //builder.RegisterType<PrinterService>()
            //    .As<IPrinterService>()
            //    .WithParameter(TypedParameter.From(appSettings))
            //    .InstancePerRequest()
            //    .PropertiesAutowired();

            // Register mock printer service instead of the real one
            builder.RegisterType<MockPrinterService>()
                .As<IPrinterService>()
                .InstancePerRequest();

            // Search service
            builder.RegisterType<SearchService>()
                .As<ISearchService>()
                .InstancePerRequest()
                .PropertiesAutowired();

            // User preference service
            builder.RegisterType<UserPreferenceService>()
                .As<IUserPreferenceService>()
                .InstancePerRequest()
                .PropertiesAutowired();

            //// SSRS integration services
            //builder.RegisterType<ReportingService>()
            //    .As<IReportingService>()
            //    .WithParameter(TypedParameter.From(appSettings))
            //    .InstancePerRequest()
            //    .PropertiesAutowired();

            //builder.RegisterType<SSRSClient>()
            //    .As<ISSRSClient>()
            //    .WithParameter(TypedParameter.From(appSettings))
            //    .InstancePerRequest()
            //    .PropertiesAutowired();
        }

        /// <summary>
        /// Registers identity components from the Infrastructure project
        /// </summary>
        private static void RegisterIdentityComponents(ContainerBuilder builder)
        {
            // --- REMOVE THIS - It's now handled by the generic registration ---
            // builder.Register(c =>
            // {
            //     var loggerFactory = c.Resolve<ILoggerFactory>();
            //     return loggerFactory.CreateLogger<UserContext>();
            // }).As<ILogger<UserContext>>().InstancePerRequest();
            // --- END REMOVAL ---

            // Windows authentication service
            // Autofac will now automatically inject ILogger<WindowsAuthenticationService>
            // into the constructor because ILoggerFactory and ILogger<> are registered.
            builder.RegisterType<WindowsAuthenticationService>()
                   .AsSelf() // Keeps registration as the concrete type
                   .InstancePerRequest()
                   .PropertiesAutowired(); // Keep if you also need property injection

            // User context
            // Autofac will also inject ILogger<UserContext> here automatically if its constructor requires it
            builder.RegisterType<UserContext>()
                   .As<IUserContext>()
                   .InstancePerRequest()
                   .PropertiesAutowired();
        }

        /// <summary>
        /// Registers components from the Web project
        /// </summary>
        public static void RegisterWebComponents(this ContainerBuilder builder)
        {
            // Register MVC controllers
            builder.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired(); // Add PropertiesAutowired here if controllers need property injection


            // Session manager - per request as it manages session state
            builder.RegisterType<SessionManager>()
                .As<ISessionManager>()
                .InstancePerRequest()
                .PropertiesAutowired();

            // User context adapter - per request as it wraps user-specific data
            builder.RegisterType<UserContextAdapter>()
                .As<IUserContextAdapter>()
                .InstancePerRequest()
                .PropertiesAutowired();
        }
    }

    /// <summary>
    /// Simple wrapper for application settings
    /// </summary>
    public interface IAppSettings
    {
        string GetSetting(string key);
        string GetConnectionString(string name);
        bool GetBoolSetting(string key, bool defaultValue = false);
        int GetIntSetting(string key, int defaultValue = 0);
    }

    /// <summary>
    /// Implementation of IAppSettings that uses ConfigurationManager
    /// </summary>
    public class AppSettingsWrapper : IAppSettings
    {
        public string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public string GetConnectionString(string name)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name];
            return connectionString?.ConnectionString;
        }

        public bool GetBoolSetting(string key, bool defaultValue = false)
        {
            var setting = GetSetting(key);
            if (string.IsNullOrEmpty(setting))
                return defaultValue;

            return bool.TryParse(setting, out bool result) ? result : defaultValue;
        }

        public int GetIntSetting(string key, int defaultValue = 0)
        {
            var setting = GetSetting(key);
            if (string.IsNullOrEmpty(setting))
                return defaultValue;

            return int.TryParse(setting, out int result) ? result : defaultValue;
        }
    }
}