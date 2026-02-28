using System;
using Cake.Frosting;
using DotAuth.Build;

return new CakeHost()
    .InstallTool(new Uri("nuget:?package=GitVersion.Tool&version=6.6.0"))
    .UseContext<BuildContext>()
    .Run(args);
