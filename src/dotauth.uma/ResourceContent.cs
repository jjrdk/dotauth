namespace DotAuth.Uma;

using System.Linq;
using System.Text.Json.Serialization;

public class ResourceContent : ResourceRegistration
{
    [JsonPropertyName("files")] public FileDetails[] Files { get; set; } = [];

    public override RegistrationData ToRegistrationData()
    {
        return new RegistrationData
        {
            Description = Description,
            IconUri = IconUri,
            Id = Id,
            Data = Files.Select(x => x.ToResource()).ToArray(),
            Metadata = Metadata,
            Owner = Owner,
            Source = Source,
            Tags = Tags,
            Name = Name,
            Type = Type,
            Scopes = Scopes,
            AccessPolicy = AccessPolicy,
            ResourceSetId = ResourceSetId,
            Registered = Registered
        };
    }
}
