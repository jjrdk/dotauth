namespace DotAuth.Build;

using Cake.Frosting;

[TaskName("Default")]
[IsDependentOn(typeof(DockerBuildTask))]
public sealed class DefaultTask : FrostingTask
{
}
