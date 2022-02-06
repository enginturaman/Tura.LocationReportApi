using MassTransit;
using System.Threading.Tasks;
using Tura.RabbitMq.Domain.Models;

namespace Tura.RabbitMq.Listener
{

    internal class LocationReportConsumer : IConsumer<LocationReport>
    {
        private readonly IDirectoryApiRepository _directoryApiRepository;

        public LocationReportConsumer(IDirectoryApiRepository directoryApiRepository)
        {
            _directoryApiRepository = directoryApiRepository;
        }
        public async Task Consume(ConsumeContext<LocationReport> context)
        {
           
            await _directoryApiRepository.ConsumeReport();
        }
    }
}