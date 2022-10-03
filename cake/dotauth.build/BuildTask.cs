namespace DotAuth.Build;

using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNetCore.MSBuild;
using Cake.Frosting;

[TaskName("Build")]
[IsDependentOn(typeof(RestoreNugetPackagesTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        var buildSettings =
            new DotNetBuildSettings
            {
                Configuration = context.BuildConfiguration,
                MSBuildSettings = new DotNetMSBuildSettings
                {
                    AssemblyVersion = context.InformationalVersion,
                    Version = context.BuildVersion,
                    FileVersion = context.InformationalVersion,
                    InformationalVersion = context.InformationalVersion,
                    PackageVersion = context.BuildVersion,
                    TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
                    WarningCodesAsMessage =
                    {
                        "618",
                        "1701",
                        "CS8600",
                        "CS8602",
                        "CS8603",
                        "NU1608"
                    }
                }
            };

        context.DotNetBuild(context.SolutionName, buildSettings);
    }
}