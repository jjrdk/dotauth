namespace DotAuth.Uma;

using System;
using System.Runtime.Serialization;

[DataContract]
public abstract class ResourceRegistration
{
    /// <summary>Gets or sets the id of the resource set.</summary>
    [DataMember(Name = "id")]
    public string Id { get; set; } = null!;

    /// <summary>Gets or sets the name.</summary>
    /// <value>The name.</value>
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the resource description.</summary>
    [DataMember(Name = "description")]
    public string? Description { get; set; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The type.</value>
    [DataMember(Name = "type")]
    public string? Type { get; set; }

    /// <summary>Gets or sets the icon URI.</summary>
    /// <value>The icon URI.</value>
    [DataMember(Name = "icon_uri")]
    public Uri? IconUri { get; set; }

    /// <summary>Gets or sets the scopes.</summary>
    /// <value>The scopes.</value>
    [DataMember(Name = "resource_scopes")]
    public string[] Scopes { get; set; } = Array.Empty<string>();
        
    /// <summary>
    /// Gets or sets the access policy <see cref="Uri"/>.
    /// </summary>
    [DataMember(Name = "access_policy")]
    public Uri? AccessPolicy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the registration
    /// </summary>
    [DataMember(Name = "registered")]
    public long Registered { get; set; }

    /// <summary>
    /// Gets or sets the resource set id.
    /// </summary>
    [DataMember(Name = "resource_set_id")]
    public string? ResourceSetId { get; set; }

    /// <summary>
    /// Gets or sets the resource owner.
    /// </summary>
    [DataMember(Name = "owner")]
    public string Owner { get; set; } = null!;

    /// <summary>
    /// Gets or sets the source of the content
    /// </summary>
    [DataMember(Name = "source")]
    public string Source { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tags for the registration.
    /// </summary>
    [DataMember(Name = "tags")]
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    [DataMember(Name = "metadata")]
    public Metadata[] Metadata { get; set; } = Array.Empty<Metadata>();

    public abstract RegistrationData ToRegistrationData();
}