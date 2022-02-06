using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace Tura.RabbitMq
{
    public class Program
    {

        private static readonly string ExecutingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        public static void Main(string[] args)
        {

            var configuration = GetConfiguration();
            Log.Logger = CreateLoggerFromSettings(configuration);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables().Build();
        }

        private static Serilog.ILogger CreateLoggerFromSettings(IConfiguration configuration) =>
            new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Source", ExecutingAssemblyName)
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
    }
}
