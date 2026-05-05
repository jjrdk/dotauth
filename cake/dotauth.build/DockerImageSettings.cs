namespace DotAuth.Build;

internal sealed class DockerImageSettings
{
    public required string Runtime { get; init; }
    public required string OutputDirectory { get; init; }
    public required string ProjectPath { get; init; }
    public required string DockerfilePath { get; init; }
    public string[] Tags { get; init; } = [];
}
