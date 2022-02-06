using Autofac;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using Tura.RabbitMq.Domain.Enums;
using Tura.RabbitMq.Infrastructures;

namespace Tura.RabbitMq.Listener
{

    public class Program
    {
        private static readonly string ExecutingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        public static Action<IConfigurationBuilder> BuildConfiguration =
            builder => builder
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();

        public static int Main(string[] args)
        {
            var configuration = GetConfiguration();
            Log.Logger = CreateLoggerFromSettings(configuration);

            try
            {
                Log.Information($"Configuring host for {ExecutingAssemblyName}.");
                var hostBuilder = CreateHostBuilder(args);

                Log.Information($"Building host for {ExecutingAssemblyName}.");
                var configuredHost = hostBuilder.Build();

                Log.Information($"Running host for {ExecutingAssemblyName}.");
                configuredHost.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Host {ExecutingAssemblyName} terminated unexpectedly.");

                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
                        Host.CreateDefaultBuilder(args)
                            .UseWindowsService()
                            .ConfigureHostConfiguration(BuildConfiguration)
                            .UseServiceProviderFactory(new AutofacServiceProviderFactory())

                            .ConfigureContainer<ContainerBuilder>(builder =>
                            {
                                builder.RegisterType<DirectoryApiRepository>().As<IDirectoryApiRepository>();

                            })

                            .ConfigureServices((hostContext, services) =>
                            {
                                IConfiguration configuration = hostContext.Configuration;

                                ConnectionWorker workerOption = configuration.GetSection("RabbitMqServerSettings").Get<ConnectionWorker>();


                                services.AddMassTransit(config =>
                                {
                                    config.AddConsumer<LocationReportConsumer>();
                                    config.UsingRabbitMq((ctx, cfg) =>
                                    {
                                        //cfg.Durable = true;
                                        cfg.Host($"amqp://{workerOption.UserName}:{workerOption.Password}@{workerOption.HostName}:{workerOption.Port}");
                                        cfg.ReceiveEndpoint(configuration.GetValue<string>("LocationReportConsumer:QueueName"), c =>
                                        {
                                            c.ConfigureConsumer<LocationReportConsumer>(ctx);
                                        });
                                    });
                                });                                
                                services.AddMassTransitHostedService();

                                Func<string, string> correctURL = delegate (string address)
                                {
                                    return address.EndsWith("/") ? address : $"{address}/";
                                };

                                services.AddHttpClient(HttpClients.AddressBookApi, (serviceProvider, client) =>
                                {
                                    client.BaseAddress = new Uri(correctURL(configuration["AddressBookApiUrl"]));
                                });

                            });

        private static IConfiguration GetConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .AddJsonFile("appsettings.json");
            return configBuilder.Build();
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
