
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

        public RabbitmqService(IConfiguration configuration)
        {
            _configuration = configuration;
            _hostName = _configuration["RabbitMQ:HostName"] ?? "rabbitmq";
            _userName = _configuration["RabbitMQ:UserName"] ?? "guest";
            _password = _configuration["RabbitMQ:Password"] ?? "guest";
            _queueName = _configuration["RabbitMQ:QueueName"] ?? "PaymentProcessedEvent";
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() { HostName = _hostName, UserName = _userName, Password = _password };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

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

            await Task.Delay(Timeout.Infinite);
        }

        public async Task EnviarMenssagem(Core.Input.PagamentoFila pagamentoFila)
        {
            var factory = new ConnectionFactory() { HostName = _hostName, UserName = _userName, Password = _password };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

           
            var mensagem = JsonSerializer.SerializeToUtf8Bytes(pagamentoFila);

            await channel.BasicPublishAsync(exchange: "", routingKey: _queueName, body: mensagem);
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
    }
}
