using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CognitoServiceSample;


class Program
{
    static async Task Main(string[] args)
    {
        // Setup configuration
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var weatherApiUrl = config["WeatherApi:Url"];

        // Setup DI
        var services = new ServiceCollection();
        services.AddHttpClient();
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        while (true)
        {
            Console.WriteLine("Press Enter to get current weather details or type 'exit' to quit...");
            var input = Console.ReadLine();
            if (input != null && input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;
            await GetWeatherAsync(httpClientFactory, weatherApiUrl);
        }
    }

    static async Task GetWeatherAsync(
        IHttpClientFactory httpClientFactory,
        string url)
    {
        var client = httpClientFactory.CreateClient();

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Weather details (JSON):");
            Console.WriteLine(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching weather: {ex.Message}");
        }
    }
}

    

