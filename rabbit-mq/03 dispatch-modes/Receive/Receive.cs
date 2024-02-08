// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
channel.BasicQos(prefetchSize:0, prefetchCount:1, global:false);
Console.WriteLine("[] Waiting For messages");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"[x] Received message {message}");

    if (message.Contains("exception"))
    {
        Console.WriteLine("Error in processing");
        channel.BasicReject(ea.DeliveryTag, false);
        throw new Exception("Error in processing");
    }

    if (int.TryParse(message, out var delayTime))
        Thread.Sleep(delayTime * 1000);
    
    // Additional processing for this message
    Console.WriteLine($"Processed message {message}");
    channel.BasicAck(deliveryTag: ea.DeliveryTag, false);
};

channel.BasicConsume("hello", autoAck: false, consumer);

Console.WriteLine("Press enter to exit");
Console.ReadLine();