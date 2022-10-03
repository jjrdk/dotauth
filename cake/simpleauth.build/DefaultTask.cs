namespace DotAuth.Build;

using Cake.Frosting;

[TaskName("Default")]
[IsDependentOn(typeof(RedisDockerBuildTask))]
public sealed class DefaultTask : FrostingTask
{
}