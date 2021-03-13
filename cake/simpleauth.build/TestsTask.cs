namespace simpleauth.build
{
    using Cake.Common.IO;
    using Cake.Common.Tools.DotNetCore;
    using Cake.Common.Tools.DotNetCore.Test;
    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Core.IO;
    using Cake.Frosting;

    [TaskName("Tests")]
    [IsDependentOn(typeof(BuildTask))]
    public class TestsTask : FrostingTask<BuildContext>
    {
        /// <inheritdoc />
        public override void Run(BuildContext context)
        {
            var projects = context.GetFiles("./tests/**/*.tests.csproj");
            projects.Add(new FilePath("./tests/simpleauth.acceptancetests/simpleauth.acceptancetests.csproj"));

            foreach (var project in projects)
            {
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
        }
    }
}