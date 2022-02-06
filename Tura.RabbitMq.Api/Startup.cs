using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Tura.RabbitMq.Infrastructures;
using Tura.RabbitMq.Producer;
using Tura.RabbitMq.Services;

namespace Tura.RabbitMq
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            services.AddScoped<IRabbitMqProducerService, RabbitMqProducerService>();
            services.AddScoped<ILocationReportService, LocationReportService>();
            services.AddScoped<IHttpContextHelper, HttpContextHelper>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            ConnectionWorker workerOption = Configuration.GetSection("RabbitMqServerSettings").Get<ConnectionWorker>();

            string rmqHost = $"amqp://{workerOption.UserName}:{workerOption.Password}@{workerOption.HostName}:{workerOption.Port}";
            Log.Information($"RabbitMq Host : {rmqHost}");
            services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rmqHost);
                    
                });
            });

            services.AddMassTransitHostedService();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tura.Directory.Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseRouting();

            app.UseAuthorization();
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "RabbitMQ Swagger");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
