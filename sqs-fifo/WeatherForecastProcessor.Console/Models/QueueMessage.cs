namespace WeatherForecastProcessor.Console.Models;

public class QueueMessage
{
    public int LoopNumber { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
