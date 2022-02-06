using MassTransit;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using Tura.RabbitMq.Domain.Models;

namespace Tura.RabbitMq.Services
{
    public interface ILocationReportService
    {
        Task SendPush();
    }
    public class LocationReportService : ILocationReportService
    {
        private readonly IPublishEndpoint _publishEndPoint;

        public LocationReportService(
            IPublishEndpoint publishEndPoint)
        {
            _publishEndPoint = publishEndPoint;
        }

        public async Task SendPush()
        {
            var locationReport = new LocationReport();
            Log.Information("Location Report olusturma isteği kuyruga iletildi.");

            await _publishEndPoint.Publish<LocationReport>(locationReport, new CancellationToken(false));
        }
    }
}
