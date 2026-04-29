namespace DotAuth.Build;

using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Docker;
using Cake.Frosting;

[TaskName("In-Memory-Docker-Build")]
[IsDependentOn(typeof(PublishWindowsAppTask))]
public sealed class InMemoryDockerBuildTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
//        DockerImageSettings[] configs = [
//            new
//            {
//                DockerfilePath = "./DockerfileInMemory",
//                OutputDirectory = "./artifacts/publish/inmemory/",
//                ProjectPath = "./src/dotauth.authserver/dotauth.authserver.csproj",
//                Runtime = "linux-musl-x64",
//                Tags = [
//                    "jjrdk/dotauth:inmemory-canary",
//                    $"jjrdk/dotauth:{context.BuildVersion}-inmemory"
//                ]
//            }
//        ];
        var publishSettings = new DotNetPublishSettings
        {
            PublishTrimmed = false,
            TieredCompilation = true,
            Runtime = "linux-musl-x64",
            SelfContained = true,
            Configuration = context.BuildConfiguration,
            OutputDirectory = "./artifacts/publish/inmemory/"
        };

        context.DotNetPublish("./src/dotauth.authserver/dotauth.authserver.csproj", publishSettings);
        var settings = new DockerImageBuildSettings
        {
            NoCache = true,
            Pull = true,
            Compress = true,
            File = "./DockerfileInMemory",
            ForceRm = true,
            Rm = true,
            Tag =
            [
                "jjrdk/dotauth:inmemory-canary", $"jjrdk/dotauth:{context.BuildVersion}-inmemory"
            ]
        };
        context.DockerBuild(settings, "./");
    }
}

internal sealed class DockerImageSettings
{
    public string Runtime { get; set; }
    public string OutputDirectory { get; set; }
    public string ProjectPath { get; set; }
    public string DockerfilePath { get; set; }
    public string[] Tags { get; set; }
}
