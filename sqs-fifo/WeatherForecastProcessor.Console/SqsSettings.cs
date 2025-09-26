namespace WeatherForecastProcessor.Console;

public class SqsSettings
{
    public string StandardQueueUrl { get; set; } = string.Empty;
    public string FifoQueueUrl { get; set; } = string.Empty;
}