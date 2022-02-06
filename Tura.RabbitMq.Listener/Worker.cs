using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;
using System.Threading.Tasks;
using Tura.RabbitMq.Infrastructures;

namespace Tura.RabbitMq.Listener
{

    public class Worker : BackgroundService
    {
        private readonly object obj = new object();
        private readonly ConnectionWorker _workerOptions;
        private readonly IConfiguration _configuration;
        private Task _executingTask;
        private CancellationTokenSource _cts;
        IConnection _connection;
        IModel _channel;
        public Worker(ConnectionWorker workerOptions, IConfiguration iconfiguration)
        {
            _workerOptions = workerOptions;
            _configuration = iconfiguration;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _workerOptions.CreateConnection();
            _channel = _connection.CreateModel();

            ConfigureConsumer(_configuration.GetValue<string>("TransactionConsumer:ExchangeName"), _configuration.GetValue<string>("TransactionConsumer:QueueName"));

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executingTask = ExecuteAsync(_cts.Token);

            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return Task.CompletedTask;
            }

            if (_channel.IsOpen) _channel.Close();
            _channel.Dispose();

            if (_connection.IsOpen) _connection.Close();
            _connection.Dispose();

            _cts.Cancel();

            Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken)).ConfigureAwait(true);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
            }
        }

        void ConfigureConsumer(string exchangeName, string queueName)
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: "direct", durable: true);

            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            _channel.QueueBind(queueName, exchangeName, $"{exchangeName}_{queueName}");

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);

            ListenRabbitMq(_channel, consumer, exchangeName, queueName);
        }

        private Task ListenRabbitMq(IModel channel, EventingBasicConsumer consumer, string exchangeName, string queueName)
        {
            lock (obj)
            {
                //_logger.Information($"Exchange: {exchangeName} Queue: {queueName} started listening...");
                //_rabbitMqListenerService.ListenRabbitMq(channel, consumer, exchangeName, queueName);
            }
            return Task.CompletedTask;
        }
    }
}
