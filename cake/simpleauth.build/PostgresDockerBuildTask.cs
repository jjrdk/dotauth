namespace SimpleAuth.Build
{
    using Cake.Common.Tools.DotNetCore;
    using Cake.Common.Tools.DotNetCore.Publish;
    using Cake.Docker;
    using Cake.Frosting;

    [TaskName("Postgres-Docker-Build")]
    [IsDependentOn(typeof(InMemoryDockerBuildTask))]
    public class PostgresDockerBuildTask : FrostingTask<BuildContext>
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
                OutputDirectory = "./artifacts/publish/postgres/"
            };

            context.DotNetCorePublish("./src/simpleauth.authserverpg/simpleauth.authserverpg.csproj", publishSettings);
            var settings = new DockerImageBuildSettings
            {
                NoCache = true,
                Pull = true,
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
        }
    }
}