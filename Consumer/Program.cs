// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using WebApiPagamento.Controllers;

Console.WriteLine("Hi!!");

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"

};

try
{
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    string queueName = "PaymentProcessedEvent";

    await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

    var consumer = new AsyncEventingBasicConsumer(channel);

    consumer.ReceivedAsync += async (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        Console.WriteLine($"Mensagem: {message}");
      
    };

    await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer
          );
}
catch (Exception e)
{
    Console.WriteLine($"Mensagem de erro: {e.Message}");
}
