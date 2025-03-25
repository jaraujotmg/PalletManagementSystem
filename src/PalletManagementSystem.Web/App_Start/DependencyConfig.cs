// src/PalletManagementSystem.Web/App_Start/DependencyConfig.cs
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Web.Mvc;
using System.Web.SessionState;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Infrastructure.Data;
using PalletManagementSystem.Infrastructure.Data.Repositories;
using PalletManagementSystem.Infrastructure.Extensions;
using PalletManagementSystem.Web.Services;

namespace PalletManagementSystem.Web.App_Start
{
    public class DependencyConfig
    {
        public static IContainer RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // Register MVC controllers
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            // Create configuration for infrastructure
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();

            // Register ConfigurationManager settings as IConfiguration
            builder.RegisterInstance(new ConfigManagerConfiguration()).As<IConfiguration>();

            // Register logger factory and loggers
            builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            // Register DbContext
            builder.Register(c => {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                return new ApplicationDbContext(optionsBuilder.Options);
            }).InstancePerRequest();

            // Register infrastructure services
            builder.RegisterType<SessionManager>().As<ISessionManager>().InstancePerRequest();
            builder.RegisterType<UserContextAdapter>().As<IUserContextAdapter>().InstancePerRequest();

            // Register infrastructure modules
            builder.RegisterModule(new InfrastructureModule());

            return builder.Build();
        }
    }

    public class InfrastructureModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register core and infrastructure services using extension methods
            builder.RegisterType<Repository<Core.Models.Pallet>>().As<IRepository<Core.Models.Pallet>>();
            builder.RegisterType<Repository<Core.Models.Item>>().As<IRepository<Core.Models.Item>>();

            // Add infrastructure services
            builder.RegisterType<Infrastructure.Data.UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.RegisterType<Infrastructure.Data.Repositories.PalletRepository>().As<IPalletRepository>().InstancePerRequest();
            builder.RegisterType<Infrastructure.Data.Repositories.ItemRepository>().As<IItemRepository>().InstancePerRequest();
            builder.RegisterType<Infrastructure.Services.TransactionManager>().As<ITransactionManager>().InstancePerRequest();

            // Add core services
            builder.RegisterType<Core.Services.PalletService>().As<IPalletService>().InstancePerRequest();
            builder.RegisterType<Core.Services.ItemService>().As<IItemService>().InstancePerRequest();
            builder.RegisterType<Core.Services.PlatformValidationService>().As<IPlatformValidationService>().InstancePerRequest();
            builder.RegisterType<Core.Services.PalletNumberGenerator>().As<IPalletNumberGenerator>().InstancePerRequest();

            // Add more infrastructure services
            builder.RegisterType<Infrastructure.Services.PrinterService>().As<IPrinterService>().InstancePerRequest();
            builder.RegisterType<Infrastructure.Services.SearchService>().As<ISearchService>().InstancePerRequest();
            builder.RegisterType<Infrastructure.Services.UserPreferenceService>().As<IUserPreferenceService>().InstancePerRequest();

            // Add SSRS integration
            builder.RegisterType<Infrastructure.Services.SSRSIntegration.ReportingService>().As<Infrastructure.Services.SSRSIntegration.IReportingService>().InstancePerRequest();
            builder.RegisterType<Infrastructure.Services.SSRSIntegration.SSRSClient>().As<Infrastructure.Services.SSRSIntegration.ISSRSClient>().InstancePerRequest();

            // Add identity services
            builder.RegisterType<Infrastructure.Identity.WindowsAuthenticationService>().AsSelf().InstancePerRequest();
            builder.RegisterType<Infrastructure.Identity.UserContext>().As<Infrastructure.Identity.IUserContext>().InstancePerRequest();
        }
    }

    // Adapter to use ConfigurationManager as IConfiguration
    public class ConfigManagerConfiguration : IConfiguration
    {
        public string this[string key]
        {
            get => ConfigurationManager.AppSettings[key];
            set => throw new System.NotImplementedException("Setting configuration values is not supported");
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new System.NotImplementedException("GetSection is not implemented in this adapter");
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new System.NotImplementedException("GetChildren is not implemented in this adapter");
        }
    }
}