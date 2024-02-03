namespace DotAuth.Uma.Client;

public class ResourceDescription
{
    public required string ResourceId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; }
}