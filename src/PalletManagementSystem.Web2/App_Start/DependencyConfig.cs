// src/PalletManagementSystem.Web/App_Start/DependencyConfig.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Infrastructure.Data;
using PalletManagementSystem.Infrastructure.Data.Repositories;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Infrastructure.Services;
using PalletManagementSystem.Infrastructure.Services.SSRSIntegration;
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

            // Create and register enhanced configuration
            var configuration = new ConfigManagerConfiguration();
            builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();

            // Register logger factory and loggers (used by multiple layers)
            builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            // Register components from each project layer
            builder.RegisterCoreComponents();
            builder.RegisterInfrastructureComponents(configuration);
            builder.RegisterWebComponents();

            return builder.Build();
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
        public static void RegisterInfrastructureComponents(this ContainerBuilder builder, IConfiguration configuration)
        {
            // Register DbContext
            builder.Register(c => {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                var connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
                optionsBuilder.UseSqlServer(connectionString);
                return new ApplicationDbContext(optionsBuilder.Options);
            })
            .InstancePerRequest() // Database context should be scoped to the request
            .PropertiesAutowired();

            // Data access components - per request lifetime to ensure
            // consistent data access within a request
            RegisterDataAccessComponents(builder);

            // External service integrations - per request lifetime as they
            // may contain request-specific configuration
            RegisterExternalServiceComponents(builder, configuration);

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
        private static void RegisterExternalServiceComponents(ContainerBuilder builder, IConfiguration configuration)
        {
            // Printer service - per request as it may use request-specific settings
            builder.RegisterType<PrinterService>()
                .As<IPrinterService>()
                .WithParameter(TypedParameter.From(configuration))
                .InstancePerRequest()
                .PropertiesAutowired();

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

            // SSRS integration services
            builder.RegisterType<ReportingService>()
                .As<IReportingService>()
                .WithParameter(TypedParameter.From(configuration))
                .InstancePerRequest()
                .PropertiesAutowired();

            builder.RegisterType<SSRSClient>()
                .As<ISSRSClient>()
                .WithParameter(TypedParameter.From(configuration))
                .InstancePerRequest()
                .PropertiesAutowired();
        }

        /// <summary>
        /// Registers identity components from the Infrastructure project
        /// </summary>
        private static void RegisterIdentityComponents(ContainerBuilder builder)
        {
            // Windows authentication service - per request as it processes the current user
            builder.RegisterType<WindowsAuthenticationService>()
                .AsSelf()
                .InstancePerRequest()
                .PropertiesAutowired();

            // User context - per request as it contains user-specific data
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
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

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
    /// Enhanced Configuration Adapter that bridges ASP.NET configuration and .NET Core configuration
    /// </summary>
    public class ConfigManagerConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string> _settings;
        private readonly Dictionary<string, ConfigSection> _sections;

        public ConfigManagerConfiguration()
        {
            // Initialize dictionaries
            _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _sections = new Dictionary<string, ConfigSection>(StringComparer.OrdinalIgnoreCase);

            // Determine environment
            var environment = ConfigurationManager.AppSettings["Environment"]
                            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                            ?? "Production";

            // Load app settings
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                _settings[key] = ConfigurationManager.AppSettings[key];
            }

            // Add environment setting
            _settings["Environment"] = environment;

            // Create ConnectionStrings section
            var connectionStringsSection = new ConfigSection("ConnectionStrings");
            foreach (ConnectionStringSettings connection in ConfigurationManager.ConnectionStrings)
            {
                connectionStringsSection.SetValue(connection.Name, connection.ConnectionString);
            }
            _sections["ConnectionStrings"] = connectionStringsSection;

            // Create AppSettings section
            var appSettingsSection = new ConfigSection("AppSettings");
            foreach (var key in _settings.Keys)
            {
                appSettingsSection.SetValue(key, _settings[key]);
            }
            _sections["AppSettings"] = appSettingsSection;
        }

        public string this[string key]
        {
            get => _settings.TryGetValue(key, out var value) ? value : null;
            set => _settings[key] = value;
        }

        public IConfigurationSection GetSection(string key)
        {
            if (_sections.TryGetValue(key, out var section))
            {
                return section;
            }

            // Create and return a new empty section
            var newSection = new ConfigSection(key);
            _sections[key] = newSection;
            return newSection;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _sections.Values;
        }

        /// <summary>
        /// Configuration section implementation for the ConfigManagerConfiguration
        /// </summary>
        public class ConfigSection : IConfigurationSection
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, ConfigSection> _children = new Dictionary<string, ConfigSection>(StringComparer.OrdinalIgnoreCase);

            public ConfigSection(string path)
            {
                Path = path;
                Key = path.Contains(":") ? path.Substring(path.LastIndexOf(':') + 1) : path;
            }

            public string this[string key]
            {
                get => _values.TryGetValue(key, out var value) ? value : null;
                set => _values[key] = value;
            }

            public string Key { get; }
            public string Path { get; }
            public string Value { get; set; }

            public void SetValue(string key, string value)
            {
                _values[key] = value;
            }

            public IConfigurationSection GetSection(string key)
            {
                if (_children.TryGetValue(key, out var section))
                {
                    return section;
                }

                var childPath = string.IsNullOrEmpty(Path) ? key : $"{Path}:{key}";
                var child = new ConfigSection(childPath);
                _children[key] = child;
                return child;
            }

            public IEnumerable<IConfigurationSection> GetChildren()
            {
                // First return child sections
                foreach (var child in _children.Values)
                {
                    yield return child;
                }

                // Then return value entries as sections
                foreach (var entry in _values)
                {
                    var childPath = string.IsNullOrEmpty(Path) ? entry.Key : $"{Path}:{entry.Key}";
                    var valueSection = new ConfigSection(childPath) { Value = entry.Value };
                    yield return valueSection;
                }
            }
        }
    }
}