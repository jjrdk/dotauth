namespace simpleauth.build
{
    using Cake.Common.Tools.DotNetCore;
    using Cake.Common.Tools.DotNetCore.MSBuild;
    using Cake.Common.Tools.DotNetCore.Pack;
    using Cake.Core.Diagnostics;
    using Cake.Frosting;

    [TaskName("Pack")]
    [IsDependentOn(typeof(RedisTestsTask))]
    public class PackTask : FrostingTask<BuildContext>
    {
        /// <inheritdoc />
        public override void Run(BuildContext context)
        {
            context.Log.Information("Package version: " + context.BuildVersion);

            var packSettings = new DotNetCorePackSettings
            {
                Configuration = context.BuildConfiguration,
                NoBuild = true,
                NoRestore = true,
                OutputDirectory = "./artifacts/packages",
                IncludeSymbols = true,
                MSBuildSettings = new DotNetCoreMSBuildSettings().SetConfiguration(context.BuildConfiguration)
                    .SetVersion(context.BuildVersion)
            };

            context.DotNetCorePack("./src/simpleauth.shared/simpleauth.shared.csproj", packSettings);
            context.DotNetCorePack("./src/simpleauth/simpleauth.csproj", packSettings);
            context.DotNetCorePack("./src/simpleauth.client/simpleauth.client.csproj", packSettings);
            context.DotNetCorePack("./src/simpleauth.stores.marten/simpleauth.stores.marten.csproj", packSettings);
            context.DotNetCorePack("./src/simpleauth.stores.redis/simpleauth.stores.redis.csproj", packSettings);
            context.DotNetCorePack("./src/simpleauth.sms/simpleauth.sms.csproj", packSettings);
            context.DotNetCorePack("./src/simpleauth.ui/simpleauth.ui.csproj", packSettings);
            context.DotNetCorePack("./src/simpleauth.sms.ui/simpleauth.sms.ui.csproj", packSettings);
        }
    }
}