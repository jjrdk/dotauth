namespace SimpleAuth.Build
{
    using Cake.Common.Tools.DotNet;
    using Cake.Common.Tools.DotNet.Publish;
    using Cake.Docker;
    using Cake.Frosting;

    [TaskName("In-Memory-Docker-Build")]
    [IsDependentOn(typeof(PublishWindowsAppTask))]
    public class InMemoryDockerBuildTask : FrostingTask<BuildContext>
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
                OutputDirectory = "./artifacts/publish/inmemory/"
            };

            context.DotNetPublish("./src/simpleauth.authserver/simpleauth.authserver.csproj", publishSettings);
            var settings = new DockerImageBuildSettings
            {
                NoCache = true,
                Pull = true,
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
        }
    }
}