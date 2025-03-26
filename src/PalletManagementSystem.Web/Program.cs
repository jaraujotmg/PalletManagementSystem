// src/PalletManagementSystem.Web/Program.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PalletManagementSystem.Web
{
    // NOTE: This file is not typically used in a traditional ASP.NET MVC 5 application running on .NET Framework 4.8.
    // It's included here for compatibility with modern tooling and to support potential future migration to ASP.NET Core.
    // The actual entry point for the application is Global.asax.cs.
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}