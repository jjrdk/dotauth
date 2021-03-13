namespace simpleauth.build
{
    using Cake.Common.Tools.DotNetCore;
    using Cake.Common.Tools.DotNetCore.Publish;
    using Cake.Docker;
    using Cake.Frosting;

    [TaskName("Docker-Build")]
    [IsDependentOn(typeof(PackTask))]
    public class DockerBuildTask : FrostingTask<BuildContext>
    {
        /// <inheritdoc />
        public override void Run(BuildContext context)
        {
            var winPublishSettings = new DotNetCorePublishSettings
            {
                PublishTrimmed = false,
                Runtime = "win-x64",
                SelfContained = true,
                Configuration = context.BuildConfiguration,
                OutputDirectory = "./artifacts/publish/winx64/"
            };

            context.DotNetCorePublish("./src/simpleauth.authserver/simpleauth.authserver.csproj", winPublishSettings);
            var publishSettings = new DotNetCorePublishSettings
            {
                PublishTrimmed = false,
                Runtime = "linux-musl-x64",
                SelfContained = true,
                Configuration = context.BuildConfiguration,
                OutputDirectory = "./artifacts/publish/inmemory/"
            };

            context.DotNetCorePublish("./src/simpleauth.authserver/simpleauth.authserver.csproj", publishSettings);
            var settings = new DockerImageBuildSettings
            {
                Compress = true,
                File = "./DockerfileInMemory",
                ForceRm = true,
                Rm = true,
                Tag = new[]
                {
                    "jjrdk/simpleauth:inmemory-canary", "jjrdk/simpleauth:" + context.BuildVersion + "-inmemory"
                }
            };
            context.DockerBuild(settings, "./");

            publishSettings.OutputDirectory = "./artifacts/publish/postgres/";

            context.DotNetCorePublish("./src/simpleauth.authserverpg/simpleauth.authserverpg.csproj", publishSettings);
            settings = new DockerImageBuildSettings
            {
                Compress = true,
                File = "./DockerfilePostgres",
                ForceRm = true,
                Rm = true,
                Tag = new[]
                {
                    "jjrdk/simpleauth:postgres-canary", "jjrdk/simpleauth:" + context.BuildVersion + "-postgres"
                }
            };
            context.DockerBuild(settings, "./");

            publishSettings.OutputDirectory = "./artifacts/publish/pgredis/";

            context.DotNetCorePublish(
                "./src/simpleauth.authserverpgredis/simpleauth.authserverpgredis.csproj",
                publishSettings);
            settings = new DockerImageBuildSettings
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