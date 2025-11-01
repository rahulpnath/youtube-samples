using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepFunctions
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new UserOnboardingUsingASLStack(app, "UserOnboardingUsingASLStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Region = "ap-southeast-2",
                }
            });

            new StepFunctionDemoStack(app, "StepFunctionDemoStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Region = "ap-southeast-2",
                }
            });

            new UserOnboardingCDKStack(app, "UserOnboardingCDKStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Region = "ap-southeast-2",
                }
            });

            new UserOnboardingCDKWithResourcesStack(app, "UserOnboardingCDKWithResourcesStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Region = "ap-southeast-2",
                }
            });

            app.Synth();
        }
    }
}
