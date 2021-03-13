namespace SimpleAuth.Build
{
    using Cake.Frosting;

    [TaskName("Default")]
    [IsDependentOn(typeof(DockerBuildTask))]
    public class DefaultTask : FrostingTask
    {
    }
}
