namespace SimpleAuth.Build
{
    using Cake.Common.Tools.DotNetCore;
    using Cake.Common.Tools.DotNetCore.Publish;
    using Cake.Docker;
    using Cake.Frosting;

    [TaskName("In-Memory-Docker-Build")]
    [IsDependentOn(typeof(PublishWindowsAppTask))]
    public class InMemoryDockerBuildTask : FrostingTask<BuildContext>
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
        }
    }
}