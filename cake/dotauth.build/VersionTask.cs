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
            var version = Assembly.GetAssembly(typeof(VersionTask))!.GetName().Version!;
            var versionString = version.ToString();
            context.Log.Information(versionString);
            versionInfo = new GitVersion
            {
                AssemblySemVer = versionString,
                BranchName = "master",
                InformationalVersion = versionString,
                FullSemVer = versionString,
                SemVer = versionString,
                LegacySemVer = versionString,
                Major = version.Major,
                Minor = version.Minor,
                Patch = version.Build,
                MajorMinorPatch = $"{version.Major}.{version.Minor}.{version.Build}",
                CommitsSinceVersionSource = 0,
                NuGetVersion = versionString
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
            context.BuildVersion =
                $"{versionInfo.MajorMinorPatch}-{versionInfo.BranchName.Replace("features/", "").Replace("_", "")}.{versionInfo.CommitsSinceVersionSource}";
        }

        if (versionInfo.BranchName == "master")
        {
            context.BuildConfiguration = "Release";
        }

        context.InformationalVersion = $"{versionInfo.MajorMinorPatch}.{(versionInfo.CommitsSinceVersionSource ?? 0)}";
        context.Log.Information($"Build configuration: {context.BuildConfiguration}");
        context.Log.Information($"Branch: {versionInfo.BranchName}");
        context.Log.Information($"Version: {versionInfo.FullSemVer}");
        context.Log.Information($"Version: {versionInfo.MajorMinorPatch}");
        context.Log.Information($"Build version: {context.BuildVersion}");
        context.Log.Information($"CommitsSinceVersionSource: {versionInfo.CommitsSinceVersionSource}");
    }
}
