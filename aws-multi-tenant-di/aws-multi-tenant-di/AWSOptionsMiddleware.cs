using Amazon.Extensions.NETCore.Setup;

namespace aws_multi_tenant_di;

public class AWSOptionsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAWSOptionsFactory optionsFactory)
    {
        optionsFactory.AWSOptionsBuilder = () =>
        {
            var awsOption = new AWSOptions();
            if (context.Request.Headers.TryGetValue("regionEndpoint", out var regionHeader))
            {
                awsOption.Region = Amazon.RegionEndpoint.GetBySystemName(regionHeader);
            }
           

            return awsOption;
        };
        
        await next(context);
    }
}