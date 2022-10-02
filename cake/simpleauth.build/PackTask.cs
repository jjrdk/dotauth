namespace SimpleAuth.Build;

using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Common.Tools.DotNetCore.MSBuild;
using Cake.Core.Diagnostics;
using Cake.Frosting;

[TaskName("Pack")]
[IsDependentOn(typeof(RedisTestsTask))]
public sealed class PackTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        context.Log.Information("Package version: " + context.BuildVersion);

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

        context.DotNetPack("./src/simpleauth.shared/simpleauth.shared.csproj", packSettings);
        context.DotNetPack("./src/simpleauth/simpleauth.csproj", packSettings);
        context.DotNetPack("./src/simpleauth.client/simpleauth.client.csproj", packSettings);
        context.DotNetPack("./src/simpleauth.stores.marten/simpleauth.stores.marten.csproj", packSettings);
        context.DotNetPack("./src/simpleauth.stores.redis/simpleauth.stores.redis.csproj", packSettings);
        context.DotNetPack("./src/simpleauth.sms/simpleauth.sms.csproj", packSettings);
        context.DotNetPack("./src/simpleauth.ui/simpleauth.ui.csproj", packSettings);
        context.DotNetPack("./src/simpleauth.sms.ui/simpleauth.sms.ui.csproj", packSettings);
    }
}