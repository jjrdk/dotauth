namespace DotAuth.Build;

using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Core.Diagnostics;
using Cake.Frosting;

[TaskName("Pack")]
[IsDependentOn(typeof(RedisTestsTask))]
public sealed class PackTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        context.Log.Information($"Package version: {context.BuildVersion}");

        var packSettings = new DotNetPackSettings
        {
            Configuration = context.BuildConfiguration,
            NoBuild = false,
            NoRestore = true,
            OutputDirectory = "./artifacts/packages",
            IncludeSymbols = true,
            MSBuildSettings = new DotNetMSBuildSettings().SetConfiguration(context.BuildConfiguration)
                .SetVersion(context.BuildVersion)
        };

        context.DotNetPack("./src/dotauth.shared/dotauth.shared.csproj", packSettings);
        context.DotNetPack("./src/dotauth/dotauth.csproj", packSettings);
        context.DotNetPack("./src/dotauth.client/dotauth.client.csproj", packSettings);
        context.DotNetPack("./src/dotauth.stores.marten/dotauth.stores.marten.csproj", packSettings);
        context.DotNetPack("./src/dotauth.stores.redis/dotauth.stores.redis.csproj", packSettings);
        context.DotNetPack("./src/dotauth.sms/dotauth.sms.csproj", packSettings);
        context.DotNetPack("./src/dotauth.ui/dotauth.ui.csproj", packSettings);
        context.DotNetPack("./src/dotauth.sms.ui/dotauth.sms.ui.csproj", packSettings);
    }
}