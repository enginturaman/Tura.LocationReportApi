using Tura.RabbitMq.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Tura.RabbitMq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationReportsController : ControllerBase
    {
        private readonly ILocationReportService _locationReportService;
        private readonly IPublishEndpoint _publishEndPoint;

        public LocationReportsController(ILocationReportService locationReportService
            )
        {
            _locationReportService = locationReportService;
        }

        [HttpPost]
        public async Task<ActionResult> Post()
        {
            await _locationReportService.SendPush();

            return Ok();

        }

    }
}
