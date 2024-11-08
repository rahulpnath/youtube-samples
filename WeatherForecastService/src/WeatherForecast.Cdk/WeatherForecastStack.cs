using Amazon.CDK;
using Constructs;

namespace WeatherForecast.Cdk
{
    public class WeatherForecastStack : Stack
    {
        internal WeatherForecastStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            
        }
    }
}