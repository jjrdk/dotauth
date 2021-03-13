namespace simpleauth.build
{
    using Cake.Common.Tools.DotNetCore;
    using Cake.Common.Tools.DotNetCore.Test;
    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Core.IO;
    using Cake.Docker;
    using Cake.Frosting;

    [TaskName("Postgres-Tests")]
    [IsDependentOn(typeof(TestsTask))]
    public class PostgresTestsTask : FrostingTask<BuildContext>
    {
        /// <inheritdoc />
        public override void Run(BuildContext context)
        {
            try
            {
                context.Log.Information("Docker compose up");

                var upsettings = new DockerComposeUpSettings
                {
                    DetachedMode = true,
                    Files = new string[] { "./tests/simpleauth.stores.marten.acceptancetests/docker-compose.yml" }
                };
                context.DockerComposeUp(upsettings);

                var project = new FilePath(
                    "./tests/simpleauth.stores.marten.acceptancetests/simpleauth.stores.marten.acceptancetests.csproj");
                context.Log.Information("Testing: " + project.FullPath);
                var reportName = "./artifacts/testreports/"
                                 + context.BuildVersion
                                 + "_"
                                 + System.IO.Path.GetFileNameWithoutExtension(project.FullPath).Replace('.', '_')
                                 + ".xml";
                reportName = System.IO.Path.GetFullPath(reportName);

                context.Log.Information(reportName);

                var coreTestSettings = new DotNetCoreTestSettings()
                {
                    NoBuild = true,
                    NoRestore = true,
                    // Set configuration as passed by command line
                    Configuration = context.BuildConfiguration,
                    ArgumentCustomization = x => x.Append("--logger \"trx;LogFileName=" + reportName + "\"")
                };

                context.DotNetCoreTest(project.FullPath, coreTestSettings);
            }
            finally
            {
                context.Log.Information("Docker compose down");

                var downsettings = new DockerComposeDownSettings
                {
                    Files = new string[] { "./tests/simpleauth.stores.marten.acceptancetests/docker-compose.yml" }
                };
                context.DockerComposeDown(downsettings);
            }
        }
    }
}