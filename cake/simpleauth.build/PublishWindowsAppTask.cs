namespace SimpleAuth.Build
{
    using Cake.Common.Tools.DotNetCore;
    using Cake.Common.Tools.DotNetCore.Publish;
    using Cake.Frosting;

    [TaskName("Publish-Windows-App")]
    [IsDependentOn(typeof(PackTask))]
    public class PublishWindowsAppTask : FrostingTask<BuildContext>
    {
        /// <inheritdoc />
        public override void Run(BuildContext context)
        {
            var winPublishSettings = new DotNetCorePublishSettings
            {
                PublishTrimmed = false,
                Runtime = "win-x64",
                SelfContained = true,
                Configuration = context.BuildConfiguration,
                OutputDirectory = "./artifacts/publish/winx64/"
            };

            context.DotNetCorePublish("./src/simpleauth.authserver/simpleauth.authserver.csproj", winPublishSettings);
        }
    }
}