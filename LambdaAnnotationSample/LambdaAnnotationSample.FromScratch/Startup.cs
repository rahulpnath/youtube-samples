using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace LambdaAnnotationSample.FromScratch
{
    [LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IMyDependency, MyDependency>();
            serviceCollection.AddTransient<Function>();
        }
    }
}
