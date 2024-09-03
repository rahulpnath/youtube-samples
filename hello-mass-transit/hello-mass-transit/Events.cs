namespace hello_mass_transit;

public class WeatherDataAddedEvent
{
    public string City { get; set; }
    public DateOnly DateTime { get; set; }
    public int TemperatureC { get; set; }
}