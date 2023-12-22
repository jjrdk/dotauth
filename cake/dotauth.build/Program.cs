using System;
using Cake.Frosting;
using DotAuth.Build;

return new CakeHost()
    .InstallTool(new Uri("nuget:?package=GitVersion.Tool&version=5.12.0"))
    .UseContext<BuildContext>()
    .Run(args);
