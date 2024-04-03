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

var exchangeName = "weather_headers";
channel.ExchangeDeclare(exchangeName, ExchangeType.Headers);
string? message = null;
string? location = null;
string? temperature = null;
do
{
    Console.WriteLine("Please enter your message");
    message = Console.ReadLine();

    Console.WriteLine("Please enter location");
    location = Console.ReadLine();
    
    Console.WriteLine("Please enter temperature");
    temperature = Console.ReadLine();
    
    SendMessage(channel, message, string.Empty);
} while (!string.IsNullOrEmpty(message));

void SendMessage(IModel channel, string message, string routingKey)
{
    var body = Encoding.UTF8.GetBytes(message);
    var basicProps = channel.CreateBasicProperties();
    basicProps.Headers = new Dictionary<string, object>()
    {
        {"location", location},
        {"temperature", temperature}
    };
    channel.BasicPublish(exchangeName, routingKey, basicProps, body);
    Console.WriteLine($"[x] Send {message}");
}