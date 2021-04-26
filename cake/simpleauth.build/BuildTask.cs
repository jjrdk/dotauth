namespace SimpleAuth.Build
{
    using System.Collections.Generic;
    using Cake.Common.Tools.DotNetCore;
    using Cake.Common.Tools.DotNetCore.MSBuild;
    using Cake.Common.Tools.MSBuild;
    using Cake.Frosting;

    [TaskName("Build")]
    [IsDependentOn(typeof(RestoreNugetPackagesTask))]
    public class BuildTask : FrostingTask<BuildContext>
    {
        /// <inheritdoc />
        public override void Run(BuildContext context)
        {
            var buildSettings = new MSBuildSettings().SetConfiguration(context.BuildConfiguration);
            buildSettings.Properties.Add("Version", new List<string> { context.BuildVersion });
            buildSettings.Properties.Add("InformationalVersion", new List<string> { context.InformationalVersion });
            //.SetVersion(context.BuildVersion)
            //.SetInformationalVersion(context.InformationalVersion);
            context.MSBuild(context.SolutionName);
            //context.DotNetCoreMSBuild(context.SolutionName, buildSettings);
        }
    }
}
