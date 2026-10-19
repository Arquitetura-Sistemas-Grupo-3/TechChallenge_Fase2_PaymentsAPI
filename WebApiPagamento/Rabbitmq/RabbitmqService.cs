
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using WebApiPagamento.NewFolder;
using Core.Input;
using Core.Repository;
using Core.Entidade;
using Core.Events;

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
        private IServiceScopeFactory _scopefactory;
        private string ExchangeName;
        private string FilaNotificacoes ;
        private string FilaCatalogo;

        private IConnection? _connection;
        private IChannel? _channel;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public RabbitmqService(IConfiguration configuration, ILogger<RabbitmqService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _hostName = configuration["RabbitMQ:Host"] ?? "rabbitmq";
            _userName = configuration["RabbitMQ:UserName"] ?? "guest";
            _password = configuration["RabbitMQ:Password"] ?? "guest";
            _queueName = configuration["RabbitMQ:QueueName"] ?? "PaymentProcessedEvent";
            _scopefactory = serviceScopeFactory;
            ExchangeName = configuration["RabbitMQ:ExchangeName"] ?? "pagamento.eventos";
            FilaNotificacoes = configuration["RabbitMQ:FilaNotificacoes"] ?? "notifications-payment-processed";
            FilaCatalogo = configuration["RabbitMQ:FilaCatalogo"] ?? "catalog-payment-processed";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try 
                {
                    var factory = new ConnectionFactory() { HostName = _hostName, UserName = _userName, Password = _password };
                    var connection = await factory.CreateConnectionAsync();
                    var channel = await connection.CreateChannelAsync();

                    await channel.QueueDeclareAsync(queue: "order-placed", durable: true, exclusive: false, autoDelete: false, arguments: null);
                    var consumer = new AsyncEventingBasicConsumer(channel);
                    string queueName = "order-placed";
                    string message = "";
                    List<Pagamento> mensagens = new List<Pagamento>();

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        message = Encoding.UTF8.GetString(body);

                        Console.WriteLine($"Mensagem: {message}");
                        _logger.LogInformation($"Mensagem recebida: {message}");
                        var pagamentoFila = JsonSerializer.Deserialize<OrderPlacedEvent>(message);
                        if (pagamentoFila != null)
                        {
                            using var scope = _scopefactory.CreateScope();
                            var repo = scope.ServiceProvider.GetRequiredService<IPagamentoRepository>();
                            var pagamento = new Core.Entidade.Pagamento
                            {
                                Status = true,
                                Valor = pagamentoFila.Price,
                                Data = DateTime.Now,
                                IdJogo = pagamentoFila.GameId,
                                IdUsuario = pagamentoFila.UserId
                            };
                            repo.Create(pagamento);
                            _logger.LogInformation($"Pagamento adicionado depois do create: Id={pagamento.Id}, Valor={pagamento.Valor}, Data={pagamento.Data}, IdJogo={pagamento.IdJogo}, IdUsuario={pagamento.IdUsuario}");
                            // _pagamentoRepository.Create(pagamento);
                            //  Console.WriteLine($"Pagamento adicionado depois do crea: Id={pagamento.Id}, Valor={pagamento.Valor}, Data={pagamento.Data}, IdJogo={pagamento.IdJogo}, IdUsuario={pagamento.IdUsuario}");
                            mensagens.Add(pagamento);



                            await Exchange(new PaymentProcessedEvent(
                                pagamentoFila.OrderId,
                                pagamentoFila.UserId,
                                pagamentoFila.GameId,
                                pagamentoFila.GameName,
                                pagamentoFila.UserEmail,
                                pagamentoFila.Price,
                                PaymentStatus.Approved));
                        }

                    };

                    await channel.BasicConsumeAsync(
                            queue: queueName,
                            autoAck: true,
                            consumer: consumer
                          );


                    await Task.Delay(Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao consumir mensagens da fila RabbitMQ. Tentando novamente em 5 segundos...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            } 
        }
     
        public async Task<List<Pagamento>> TesteAsync()
        {
            var factory = new ConnectionFactory() { HostName = _hostName, UserName = _userName, Password = _password };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new AsyncEventingBasicConsumer(channel);
            
            string message = "";
            List<Pagamento> mensagens = new List<Pagamento>();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Mensagem: {message}");

                    var pagamentoFila = JsonSerializer.Deserialize<OrderPlacedEvent>(message);
                    if (pagamentoFila != null)
                    {
                    using var scope = _scopefactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IPagamentoRepository>();
                    var pagamento = new Core.Entidade.Pagamento
                        {
                            Status = true,
                            Valor = pagamentoFila.Price,
                            Data = DateTime.Now,
                            IdJogo = pagamentoFila.GameId,
                            IdUsuario = pagamentoFila.UserId
                        };
                        repo.Create(pagamento);
                    // _pagamentoRepository.Create(pagamento);
                    Console.WriteLine($"Pagamento adicionado: Id={pagamento.Id}, Valor={pagamento.Valor}, Data={pagamento.Data}, IdJogo={pagamento.IdJogo}, IdUsuario={pagamento.IdUsuario}");
                       mensagens.Add(pagamento);
                
                    }
              
            };

            await channel.BasicConsumeAsync(
                    queue: _queueName,
                    autoAck: true,
                    consumer: consumer
                  );
            await Task.Delay(1000);

            return mensagens;
        }

        public async Task EnviarMensagemFilaOrder(OrderPlacedEvent orderPlacedEvent)
        {
            var factory = new ConnectionFactory() { HostName = _hostName, UserName = _userName, Password = _password };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            _logger.LogInformation($"Enviando mensagem para a fila: {JsonSerializer.Serialize(orderPlacedEvent)}");
            await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var body = JsonSerializer.SerializeToUtf8Bytes(orderPlacedEvent);
            var props = new BasicProperties { Persistent = true }; 
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _queueName,
                mandatory: false,
                body: body,
                basicProperties: props);
            _logger.LogInformation($"Mensagem enviada para a fila: {JsonSerializer.Serialize(orderPlacedEvent)}");
        }
       
        public async Task Exchange(PaymentProcessedEvent pagamentoFila)
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

         
                await _channel.ExchangeDeclareAsync(
                    exchange: ExchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: ct);


                foreach (var fila in new[] { FilaNotificacoes, FilaCatalogo })
                {
                    await _channel.QueueDeclareAsync(
                        queue: fila,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: ct);

              
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
