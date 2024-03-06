// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;

Console.WriteLine("Hello, World!");

var factory = new ConnectionFactory()
{
    Uri = new Uri("YOUR RABBIT INSTANCE URI"),
    Port = 5671,
    UserName = "<USERNAME FROM CONFIGURATION FILE>",
    Password = "<PASSWORD FROM CONFIGURATION FILE>"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// channel.QueueDeclare("hello", false, false, false, null);

var exchangeName = "weather_fanout";
channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
string? message = null;

do
{
    Console.WriteLine("Please enter your message");
    message = Console.ReadLine();
    
    Console.WriteLine("Please enter your routing key");
    var routingKey = Console.ReadLine();
    if (!string.IsNullOrEmpty(message))
        SendMessage(channel, message, routingKey ?? string.Empty);
} while (!string.IsNullOrEmpty(message));

void SendMessage(IModel channel, string message, string routingKey)
{
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(exchangeName, routingKey, null, body);
    Console.WriteLine($"[x] Send {message}");
}