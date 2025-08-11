using System.Text.Json.Serialization;

namespace CognitoServiceSample;

public class TokenHelper
{
    public static async Task<string?> GetAccessTokenAsync(HttpClient client, ClientCredentialsOptions clientCredentialsOptions)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientCredentialsOptions.ClientId),
            new KeyValuePair<string, string>("client_secret", clientCredentialsOptions.ClientSecret)
        });
        var tokenResponse = await client.PostAsync(clientCredentialsOptions.TokenEndpoint, content);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"Token error: {tokenResponse.StatusCode}");
            return null;
        }

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        var tokenObj = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(tokenJson);
        return tokenObj?.AccessToken;
    }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }
    [JsonPropertyName("token_type")] public string TokenType { get; set; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}

public class ClientCredentialsOptions
{
    public string TokenEndpoint { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}