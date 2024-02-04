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

channel.QueueDeclare("hello", false, false, false, null);

string? message = null;

do
{
    Console.WriteLine("Please enter your message");
    message = Console.ReadLine();
    if (!string.IsNullOrEmpty(message))
        SendMessage(channel, message);
} while (!string.IsNullOrEmpty(message));

void SendMessage(IModel channel, string message)
{
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(string.Empty, "hello", null, body);
    Console.WriteLine($"[x] Send {message}");
}