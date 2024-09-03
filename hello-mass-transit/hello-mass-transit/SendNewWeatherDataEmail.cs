using MassTransit;

namespace hello_mass_transit;

public class SendNewWeatherDataEmail: IConsumer<WeatherDataAddedEvent>
{
    public Task Consume(ConsumeContext<WeatherDataAddedEvent> context)
    {
        Console.WriteLine($"Sending Email for City {context.Message.City} on {context.Message.DateTime} with {context.Message.TemperatureC}");
        return Task.CompletedTask;
    }
}

public class SendNewWeatherDataSMS: IConsumer<WeatherDataAddedEvent>
{
    public Task Consume(ConsumeContext<WeatherDataAddedEvent> context)
    {
        Console.WriteLine($"Sending SMS for City {context.Message.City} on {context.Message.DateTime} with {context.Message.TemperatureC}");
        return Task.CompletedTask;
    }
}