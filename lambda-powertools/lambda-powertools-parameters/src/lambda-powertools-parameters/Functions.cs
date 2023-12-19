using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Parameters;
using AWS.Lambda.Powertools.Parameters.Transform;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lambda_powertools_parameters
{
    public class Functions
    {
        [LambdaFunction(Role = "@ParametersApiLambdaExecutionRole")]
        [HttpApi(LambdaHttpMethod.Get, "/parameters")]
        public async Task<List<string>> GetParameters()
        {
            var parameters = new List<string>();
            parameters.Add(FormatKeyValue("key", "value"));

            var value1 = await ParametersManager.SsmProvider
                .WithMaxAge(TimeSpan.FromSeconds(10))
                .GetAsync("/Value1");

            parameters.Add(FormatKeyValue("Value1", value1));

            var multipleValues = await ParametersManager.SsmProvider.GetMultipleAsync("/weather-app/");
            foreach (var multipleValue in multipleValues)
            {
                parameters.Add(FormatKeyValue(multipleValue.Key, multipleValue.Value));
            }

            var configuration = await ParametersManager.SsmProvider
                .WithTransformation(Transformation.Json)
                .GetAsync<MyConfiguration>("/my-configuration");

            parameters.Add(FormatKeyValue("my-configuration", $"{configuration.Url}:{configuration.Secret}"));

            var secret1 = await ParametersManager.SecretsProvider.GetAsync("weather-app/secret1");
            parameters.Add(FormatKeyValue("weather-app/secret1", secret1));

            return parameters;
        }

        public string FormatKeyValue(string key, string value) => $"{key} -- {value}";
    }

    public class MyConfiguration
    {
        public string Secret { get; set; }
        public string Url { get; set; }
    }
}
