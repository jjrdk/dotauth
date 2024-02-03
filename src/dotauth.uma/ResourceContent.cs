namespace DotAuth.Uma;

using System;
using System.Linq;
using System.Runtime.Serialization;

public class ResourceContent : ResourceRegistration
{
    [DataMember(Name = "files")] public FileDetails[] Files { get; set; } = Array.Empty<FileDetails>();

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
