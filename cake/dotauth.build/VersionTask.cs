namespace DotAuth.Build;

using System.Reflection;
using Cake.Common.Tools.GitVersion;
using Cake.Core.Diagnostics;
using Cake.Frosting;

[TaskName("Version")]
[TaskDescription("Retrieves the current version from the git repository")]
public sealed class VersionTask : FrostingTask<BuildContext>
{
    // Tasks can be asynchronous
    public override async void Run(BuildContext context)
    {
        GitVersion versionInfo;
        try
        {
            versionInfo = context.GitVersion(new GitVersionSettings { UpdateAssemblyInfo = false });
        }
        catch
        {
            var assembly = Assembly.GetAssembly(typeof(VersionTask))!.GetName().Version;
            context.Log.Information(assembly.ToString());
            versionInfo = new GitVersion
            {
                AssemblySemVer = assembly.ToString(),
                BranchName = "master",
                InformationalVersion = assembly.ToString(),
                FullSemVer = assembly.ToString(),
                SemVer = assembly.ToString(),
                LegacySemVer = assembly.ToString(),
                Major = assembly.Major,
                Minor = assembly.Minor,
                Patch = assembly.Build,
                MajorMinorPatch = $"{assembly.Major}.{assembly.Minor}.{assembly.Build}",
                CommitsSinceVersionSource = 0,
                NuGetVersion = assembly.ToString()
            };
        }
        if (versionInfo.BranchName == "master" || versionInfo.BranchName.StartsWith("tags/"))
        {
            context.BuildVersion =
                versionInfo.CommitsSinceVersionSource is > 0
                    ? $"{versionInfo.MajorMinorPatch}-beta.{versionInfo.CommitsSinceVersionSource.Value}"
                    : versionInfo.MajorMinorPatch;
        }
        else
        {
            context.BuildVersion = string.Concat(
                versionInfo.MajorMinorPatch,
                "-",
                versionInfo.BranchName.Replace("features/", "").Replace("_", ""),
                ".",
                versionInfo.CommitsSinceVersionSource);
        }

        if (versionInfo.BranchName == "master")
        {
            context.BuildConfiguration = "Release";
        }

        context.InformationalVersion = versionInfo.MajorMinorPatch + "." + (versionInfo.CommitsSinceVersionSource ?? 0);
        context.Log.Information("Build configuration: " + context.BuildConfiguration);
        context.Log.Information("Branch: " + versionInfo.BranchName);
        context.Log.Information("Version: " + versionInfo.FullSemVer);
        context.Log.Information("Version: " + versionInfo.MajorMinorPatch);
        context.Log.Information("Build version: " + context.BuildVersion);
        context.Log.Information("CommitsSinceVersionSource: " + versionInfo.CommitsSinceVersionSource);
    }
}
