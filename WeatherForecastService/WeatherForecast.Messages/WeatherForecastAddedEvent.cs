namespace WeatherForecast.Messages
{
    public class WeatherForecastAddedEvent
    {
        public string City { get; set; }
        public DateTime DateTime { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }
    }
}
