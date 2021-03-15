namespace SimpleAuth.Build
{
    using Cake.Frosting;

    [TaskName("Default")]
    [IsDependentOn(typeof(RedisDockerBuildTask))]
    public class DefaultTask : FrostingTask
    {
    }
}
