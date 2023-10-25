namespace apple_wallet
{
    [Amazon.Lambda.Annotations.LambdaStartup]
    public class Startup
    {

        public void ConfigureServices(IServiceCollection services)
        {
            //// Example of creating the IConfiguration object and
            //// adding it to the dependency injection container.
            var builder = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", true);

            //// Add AWS Systems Manager as a potential provider for the configuration. This is 
            //// available with the Amazon.Extensions.Configuration.SystemsManager NuGet package.
            //builder.AddSystemsManager("/app/settings");

            var configuration = builder.Build();
            services.AddSingleton<IConfiguration>(configuration);

            services.AddOptions<AppleWalletConfiguration>().BindConfiguration(nameof(AppleWalletConfiguration));

            //// Example of using the AWSSDK.Extensions.NETCore.Setup NuGet package to add
            //// the Amazon S3 service client to the dependency injection container.
            //services.AddAWSService<Amazon.S3.IAmazonS3>();
        }
    }
}
