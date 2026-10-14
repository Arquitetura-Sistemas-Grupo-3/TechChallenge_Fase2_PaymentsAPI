
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using WebApiPagamento.NewFolder;
using Core.Input;

namespace WebApiPagamento.Rabbitmq
{
    public class RabbitmqService : BackgroundService, IRabbitmqService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitmqService> _logger;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;  
        private readonly string _queueName;
        private const string ExchangeName = "pagamento.eventos";
        private const string FilaNotificacoes = "notifications-payment-processed";
        private const string FilaCatalogo = "payment-processed";


        private IConnection? _connection;
        private IChannel? _channel;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public RabbitmqService(IConfiguration configuration, ILogger<RabbitmqService> logger)
        {
            _logger = logger;
            _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
            _userName = configuration["RabbitMQ:UserName"] ?? "guest";
            _password = configuration["RabbitMQ:Password"] ?? "guest";
            _queueName = configuration["RabbitMQ:QueueName"] ?? "PaymentProcessedEvent";
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() { HostName = _hostName, UserName = _userName, Password = _password };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new AsyncEventingBasicConsumer(channel);
            string queueName = "order-placed";
            string message = "";
            List<String> mensagens = new List<String>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Mensagem: {message}");
                mensagens.Add(message);
            };

            await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: true,
                    consumer: consumer
                  );

            await Task.Delay(Timeout.Infinite);
        }

        public async Task EnviarMenssagem(Core.Input.PagamentoFila pagamentoFila)
        {
            var channel = await GetChannelAsync();

            var body = JsonSerializer.SerializeToUtf8Bytes(pagamentoFila);
            var props = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: "",
                mandatory: false,
                basicProperties: props,
                body: body);
        }
        /*
        public async Task<List<String>> ConsumirMensagem()
        {
            var factory = new ConnectionFactory() { HostName = _hostName, UserName = _userName, Password = _password };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new AsyncEventingBasicConsumer(channel);
            string queueName = "OrderPlacedQueue";
            string message = "";
            List<String> mensagens = new List<String>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Mensagem: {message}");
                mensagens.Add(message);
            };

            await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: true,
                    consumer: consumer
                  );

            return mensagens;   
        }*/

        private async Task<IChannel> GetChannelAsync(CancellationToken ct = default)
        {
            if (_channel is { IsOpen: true })
                return _channel;

            await _initLock.WaitAsync(ct);
            try
            {
                if (_channel is { IsOpen: true })
                    return _channel;

                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password
                };

                _connection = await factory.CreateConnectionAsync(ct);
                _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

                // 1) Exchange fanout
                await _channel.ExchangeDeclareAsync(
                    exchange: ExchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: ct);

                // 2) Filas
                foreach (var fila in new[] { FilaNotificacoes, FilaCatalogo })
                {
                    await _channel.QueueDeclareAsync(
                        queue: fila,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: ct);

                    // 3) Bind: liga a fila ao exchange (routingKey é ignorada no fanout)
                    await _channel.QueueBindAsync(
                        queue: fila,
                        exchange: ExchangeName,
                        routingKey: "",
                        cancellationToken: ct);
                }

                return _channel;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}
