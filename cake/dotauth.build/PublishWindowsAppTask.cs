namespace DotAuth.Build;

using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Frosting;

[TaskName("Publish-Windows-App")]
[IsDependentOn(typeof(PackTask))]
public sealed class PublishWindowsAppTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        var winPublishSettings = new DotNetPublishSettings
        {
            PublishTrimmed = false,
            TieredCompilation = true,
            Runtime = "win-x64",
            SelfContained = true,
            Configuration = context.BuildConfiguration,
            OutputDirectory = "./artifacts/publish/winx64/"
        };

        context.DotNetPublish("./src/dotauth.authserver/dotauth.authserver.csproj", winPublishSettings);
    }
}
