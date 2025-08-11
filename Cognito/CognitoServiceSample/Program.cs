using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CognitoServiceSample;


class Program
{
    static async Task Main(string[] args)
    {
        // Setup configuration
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>() // Load user secrets for development
            .Build();

        var weatherApiUrl = config["WeatherApi:Url"];

        // Bind OAuth2 section to ClientCredentialsOptions
        var clientCredentialsOptions = new ClientCredentialsOptions();
        config.GetSection("OAuth2").Bind(clientCredentialsOptions);

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
            await GetWeatherAsync(httpClientFactory, weatherApiUrl, clientCredentialsOptions);
        }
    }

    static async Task GetWeatherAsync(
        IHttpClientFactory httpClientFactory,
        string url,
        ClientCredentialsOptions options)
    {
        var client = httpClientFactory.CreateClient();

        try
        {
            var token = await TokenHelper.GetAccessTokenAsync(client, options);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
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

