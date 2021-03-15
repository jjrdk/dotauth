namespace SimpleAuth.Build
{
    using Cake.Common.Tools.DotNetCore;
    using Cake.Common.Tools.DotNetCore.Publish;
    using Cake.Docker;
    using Cake.Frosting;

    [TaskName("Redis-Docker-Build")]
    [IsDependentOn(typeof(PostgresDockerBuildTask))]
    public class RedisDockerBuildTask : FrostingTask<BuildContext>
    {
        /// <inheritdoc />
        public override void Run(BuildContext context)
        {
            var publishSettings = new DotNetCorePublishSettings
            {
                PublishTrimmed = false,
                Runtime = "linux-musl-x64",
                SelfContained = true,
                Configuration = context.BuildConfiguration,
                OutputDirectory = "./artifacts/publish/pgredis/"
            };

            context.DotNetCorePublish(
                "./src/simpleauth.authserverpgredis/simpleauth.authserverpgredis.csproj",
                publishSettings);
            var settings = new DockerImageBuildSettings
            {
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
}