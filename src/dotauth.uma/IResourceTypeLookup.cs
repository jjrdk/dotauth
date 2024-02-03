namespace DotAuth.Uma;

using System.Threading.Tasks;

/// <summary>
/// Defines the resource type lookup interface
/// </summary>
public interface IResourceTypeLookup
{
    /// <summary>
    /// Gets the defined resource type for the corresponding mime type.
    /// </summary>
    /// <param name="mimeType">The mime type of the resource.</param>
    /// <returns></returns>
    Task<string> GetResourceType(string mimeType);

    /// <summary>
    /// Gets the file extension for the resource mime type.
    /// </summary>
    /// <param name="mimeType">The mime type of the resource.</param>
    /// <returns></returns>
    Task<string> GetExtension(string mimeType);
}