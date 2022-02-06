using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using Tura.RabbitMq.Infrastructures;

namespace Tura.RabbitMq.Producer
{
    public interface IRabbitMqProducerService
    {
        void Push(ConnectionWorker workerOptions,
            string channelName, string exchangeName, object model);
    }

    public class RabbitMqProducerService : IRabbitMqProducerService
    {
        public void Push(ConnectionWorker workerOptions,
            string channelName,
            string exchangeName,
            object model)
        {
            using (var connection = workerOptions.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchangeName, type: "direct", durable: true);

                    channel.QueueDeclare(
                        queue: channelName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);

                    channel.QueueBind(
                        queue: channelName,
                        exchange: exchangeName,
                        routingKey: $"{exchangeName}_{channelName}",
                        arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    var stocData = JsonConvert.SerializeObject(model);
                    var body = Encoding.UTF8.GetBytes(stocData);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(
                        exchange: exchangeName,
                        routingKey: $"{exchangeName}_{channelName}",
                        basicProperties: properties,
                        body: body);
                }
            }
        }
    }
}
