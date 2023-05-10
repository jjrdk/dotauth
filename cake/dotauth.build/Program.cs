using System;
using Cake.Frosting;
using DotAuth.Build;

return new CakeHost()
    .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.12.0"))
    .InstallTool(new Uri("nuget:?package=Cake.Docker&version=1.1.2"))
    .UseContext<BuildContext>()
    .Run(args);
