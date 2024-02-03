namespace DotAuth.Uma;

using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Defines the fixed resource type lookup
/// </summary>
public class FixedResourceTypeLookup : IResourceTypeLookup
{
    private static readonly string[] StandardFiles = ["image", "video", "font", "text"];

    /// <inheritdoc />
    public Task<string> GetResourceType(string mimeType)
    {
        var resource = mimeType.Split('/');
        return resource[0] switch
        {
            "image" => Task.FromResult("Picture"),
            "video" => Task.FromResult("Picture"),
            "font" => Task.FromResult("Font"),
            "text" => Task.FromResult($"{resource[1]} {resource[0]}"),
            "application" => Task.FromResult("Binary"),
            _ => Task.FromResult("Unknown")
        };
    }

    /// <inheritdoc />
    public Task<string> GetExtension(string mimeType)
    {
        var extension = MimeTypeMap.GetExtension(mimeType);
        if (!string.IsNullOrWhiteSpace(extension))
        {
            return Task.FromResult(extension);
        }
        var parts = mimeType.Split('/');
        return parts[0] switch
        {
            _ when StandardFiles.Contains(parts[0]) => Task.FromResult($".{parts[1]}"),
            _ => Task.FromResult(string.Empty)
        };
    }
}