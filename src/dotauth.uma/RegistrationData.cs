namespace DotAuth.Uma;

using System.Text.Json.Serialization;

public class RegistrationData : ResourceRegistration
{
    /// <summary>
    /// Gets or sets the content description
    /// </summary>
    [JsonPropertyName("files")]
    public FileDescription[] Data { get; set; } = [];

    public override RegistrationData ToRegistrationData()
    {
        return this;
    }
}
