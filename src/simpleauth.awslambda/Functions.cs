using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SimpleAuth.AwsLambda
{
    using Amazon.Lambda.AspNetCoreServer;
    using Microsoft.AspNetCore.Hosting;
    using System.IO;

    public class Functions : APIGatewayProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory()).UseStartup<Startup>().UseApiGateway();
        }
    }
}
