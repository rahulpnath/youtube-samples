using Amazon.Extensions.NETCore.Setup;

namespace aws_multi_tenant_di;

public interface IAWSOptionsFactory
{
    Func<AWSOptions> AWSOptionsBuilder { get; set; }
}

public class AWSOptionsFactory: IAWSOptionsFactory
{
    public Func<AWSOptions> AWSOptionsBuilder { get; set; }
}