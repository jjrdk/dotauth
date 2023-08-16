namespace DotAuth.Build;

using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Test;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Docker;
using Cake.Frosting;

[TaskName("Postgres-Tests")]
[IsDependentOn(typeof(TestsTask))]
public sealed class PostgresTestsTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        var dockerComposeFiles = new [] { "./tests/dotauth.stores.marten.acceptancetests/docker-compose.yml" };
        try
        {
            context.Log.Information("Docker compose up");
            context.Log.Information("Ensuring test report output");

            context.EnsureDirectoryExists(context.Environment.WorkingDirectory.Combine("artifacts").Combine("testreports"));

            var upsettings = new DockerComposeUpSettings
            {
                Detach = true,
                Files = dockerComposeFiles
            };
            context.DockerComposeUp(upsettings);

            var project = new FilePath(
                "./tests/dotauth.stores.marten.acceptancetests/dotauth.stores.marten.acceptancetests.csproj");
            context.Log.Information("Testing: " + project.FullPath);
            var reportName = "./artifacts/testreports/"
                             + context.BuildVersion
                             + "_"
                             + System.IO.Path.GetFileNameWithoutExtension(project.FullPath)!.Replace('.', '_')
                             + ".xml";
            reportName = System.IO.Path.GetFullPath(reportName);

            context.Log.Information(reportName);

            var coreTestSettings = new DotNetTestSettings()
            {
                NoBuild = true,
                NoRestore = true,
                // Set configuration as passed by command line
                Configuration = context.BuildConfiguration,
                ArgumentCustomization = x => x.Append("--logger \"trx;LogFileName=" + reportName + "\"")
            };

            context.DotNetTest(project.FullPath, coreTestSettings);
        }
        finally
        {
            context.Log.Information("Docker compose down");

            var downsettings = new DockerComposeDownSettings
            {
                Files = dockerComposeFiles
            };
            context.DockerComposeDown(downsettings);
        }
    }
}
