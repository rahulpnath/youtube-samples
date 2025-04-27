using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace AwsEc2;

public class AwsEc2Stack : Stack
{
    internal AwsEc2Stack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        // Lookup the default VPC
        var vpc = Vpc.FromLookup(this, "DefaultVpc", new VpcLookupOptions
        {
            IsDefault = true
        });

        // Create a security group
        var securityGroup = new SecurityGroup(this, "WebApiSecurityGroup", new SecurityGroupProps
        {
            Vpc = vpc,
            AllowAllOutbound = true,
            Description = "Allow SSH and HTTP",
            SecurityGroupName = "WebApiSecurityGroup"
        });

        securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(22), "Allow SSH access");
        securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(80), "Allow HTTP access");
        securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(5000), "MyApi App access");

        // Role for EC2
        var role = new Role(this, "Ec2Role", new RoleProps
        {
            AssumedBy = new ServicePrincipal("ec2.amazonaws.com")
        });

        role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonSSMManagedInstanceCore"));

        // Create EC2 Instance
        var instance = new Instance_(this, "WebApiInstance", new InstanceProps
        {
            InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MICRO),
            MachineImage = MachineImage.LatestAmazonLinux2023(),
            Vpc = vpc,
            Role = role,
            SecurityGroup = securityGroup,
            KeyPair = KeyPair.FromKeyPairName(this, "key-0a498c763ef8ab954", "my-key-pair"),
        });

        new CfnOutput(this, "InstancePublicIp", new CfnOutputProps
        {
            Value = instance.InstancePublicIp,
            Description = "Public IP of the EC2 instance"
        });
    }
}