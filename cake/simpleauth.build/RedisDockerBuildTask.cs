namespace SimpleAuth.Build;

using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Docker;
using Cake.Frosting;

[TaskName("Redis-Docker-Build")]
[IsDependentOn(typeof(PostgresDockerBuildTask))]
public sealed class RedisDockerBuildTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        var publishSettings = new DotNetPublishSettings
        { 
            PublishTrimmed = false,
            Runtime = "linux-musl-x64",
            SelfContained = true,
            Configuration = context.BuildConfiguration,
            OutputDirectory = "./artifacts/publish/pgredis/"
        };

        context.DotNetPublish(
            "./src/simpleauth.authserverpgredis/simpleauth.authserverpgredis.csproj",
            publishSettings);
        var settings = new DockerImageBuildSettings
        {
            NoCache = true,
            Pull = true,
            Compress = true,
            File = "./DockerfilePgRedis",
            ForceRm = true,
            Rm = true,
            Tag = new[]
            {
                "jjrdk/simpleauth:pgredis-canary", "jjrdk/simpleauth:" + context.BuildVersion + "-pgredis"
            }
        };
        context.DockerBuild(settings, "./");
    }
}