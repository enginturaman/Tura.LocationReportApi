using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tura.RabbitMq.Domain.Enums;

namespace Tura.RabbitMq.Listener
{
    public interface IDirectoryApiRepository
    {
        Task<bool> ConsumeReport();
    }
    public class DirectoryApiRepository : IDirectoryApiRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DirectoryApiRepository(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<bool> ConsumeReport()
        {
            using (var client = _httpClientFactory.CreateClient(HttpClients.AddressBookApi))
            {

                using (var response = await client.PostAsync($"{client.BaseAddress}LocationReports/CreateLocationReport", null))
                {
                    using (HttpContent content = response.Content)
                    {
                        var json = content.ReadAsStringAsync().Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return true;
                        }
                        else
                        {
                            throw new Exception($"ConsumeReport - RMQ Api Servis Hatası : {json}");
                        }
                    }
                }
            }
        }
    }
}
