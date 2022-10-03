using System;
using Cake.Frosting;
using DotAuth.Build;

return new CakeHost()
    .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.10.3"))
    .InstallTool(new Uri("nuget:?package=Cake.Docker&version=1.1.2"))
    .UseContext<BuildContext>()
    .Run(args);