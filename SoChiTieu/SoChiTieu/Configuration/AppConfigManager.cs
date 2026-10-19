using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SoChiTieu.Configuration
{
    public static class AppConfigManager
    {
        public static AppSettings LoadSettings()
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                      ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                      ?? "Development";

            var basePath = AppContext.BaseDirectory;

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);

            var configRoot = builder.Build();
            var settings = new AppSettings();
            configRoot.Bind(settings);

            if (string.IsNullOrWhiteSpace(settings.ConnectionStrings?.DefaultConnection))
            {
                var conn = configRoot.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrWhiteSpace(conn))
                {
                    settings.ConnectionStrings ??= new ConnectionStringsOptions();
                    settings.ConnectionStrings.DefaultConnection = conn;
                }
            }

            return settings;
        }

        public static string GetConnectionString()
        {
            var settings = LoadSettings();
            return settings.ConnectionStrings?.DefaultConnection ?? string.Empty;
        }
    }
}
