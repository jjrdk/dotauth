namespace DotAuth.Build;

using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Docker;
using Cake.Frosting;

[TaskName("In-Memory-Docker-Build")]
[IsDependentOn(typeof(PublishWindowsAppTask))]
public sealed class DockerBuildTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        DockerImageSettings[] configs =
        [
            new()
            {
                DockerfilePath = "./DockerfileInMemory",
                OutputDirectory = "./artifacts/publish/inmemory/musl/",
                ProjectPath = "./src/dotauth.authserver/dotauth.authserver.csproj",
                Runtime = "linux-musl-x64",
                Tags =
                [
                    "jjrdk/dotauth:inmemory-canary-alpine",
                    $"jjrdk/dotauth:{context.BuildVersion}-inmemory-alpine"
                ]
            },
            new()
            {
                DockerfilePath = "./DockerfileInMemoryChiseled",
                OutputDirectory = "./artifacts/publish/inmemory/chiseled/",
                ProjectPath = "./src/dotauth.authserver/dotauth.authserver.csproj",
                Runtime = "linux-x64",
                Tags =
                [
                    "jjrdk/dotauth:inmemory-canary-chiseled",
                    $"jjrdk/dotauth:{context.BuildVersion}-inmemory-chiseled",
                    "jjrdk/dotauth:inmemory-canary",
                    $"jjrdk/dotauth:{context.BuildVersion}-inmemory"
                ]
            },
            new()
            {
                DockerfilePath = "./DockerfilePostgres",
                OutputDirectory = "./artifacts/publish/postgres/musl/",
                ProjectPath = "./src/dotauth.authserverpg/dotauth.authserverpg.csproj",
                Runtime = "linux-musl-x64",
                Tags =
                [
                    "jjrdk/dotauth:postgres-canary-alpine", $"jjrdk/dotauth:{context.BuildVersion}-postgres-alpine"
                ]
            },
            new()
            {
                DockerfilePath = "./DockerfilePostgresChiseled",
                OutputDirectory = "./artifacts/publish/postgres/chiseled/",
                ProjectPath = "./src/dotauth.authserverpg/dotauth.authserverpg.csproj",
                Runtime = "linux-x64",
                Tags =
                [
                    "jjrdk/dotauth:postgres-canary-chiseled", $"jjrdk/dotauth:{context.BuildVersion}-postgres-chiseled",
                    "jjrdk/dotauth:postgres-canary", $"jjrdk/dotauth:{context.BuildVersion}-postgres"
                ]
            },
            new()
            {
                DockerfilePath = "./DockerfilePgRedis",
                OutputDirectory = "./artifacts/publish/pgredis/musl/",
                ProjectPath = "./src/dotauth.authserverpgredis/dotauth.authserverpgredis.csproj",
                Runtime = "linux-musl-x64",
                Tags =
                [
                    "jjrdk/dotauth:pgredis-canary-alpine", $"jjrdk/dotauth:{context.BuildVersion}-pgredis-alpine"
                ]
            },
            new()
            {
                DockerfilePath = "./DockerfilePgRedisChiseled",
                OutputDirectory = "./artifacts/publish/pgredis/chiseled/",
                ProjectPath = "./src/dotauth.authserverpgredis/dotauth.authserverpgredis.csproj",
                Runtime = "linux-x64",
                Tags =
                [
                    "jjrdk/dotauth:pgredis-canary-chiseled",
                    $"jjrdk/dotauth:{context.BuildVersion}-pgredis-chiseled",
                    "jjrdk/dotauth:pgredis-canary",
                    $"jjrdk/dotauth:{context.BuildVersion}-pgredis",
                ]
            },
        ];
        foreach (var config in configs)
        {
            var publishSettings = new DotNetPublishSettings
            {
                PublishTrimmed = false,
                TieredCompilation = true,
                Runtime = config.Runtime,
                SelfContained = true,
                Configuration = context.BuildConfiguration,
                OutputDirectory = config.OutputDirectory
            };

            context.DotNetPublish(config.ProjectPath, publishSettings);
            var settings = new DockerImageBuildSettings
            {
                NoCache = true,
                Pull = true,
                Compress = true,
                File = config.DockerfilePath,
                ForceRm = true,
                Rm = true,
                Tag = config.Tags
            };
            context.DockerBuild(settings, "./");
        }
    }
}
