namespace DotAuth.Uma;

using System;
using System.Text.Json.Serialization;

public abstract class ResourceRegistration
{
    /// <summary>Gets or sets the id of the resource set.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>Gets or sets the name.</summary>
    /// <value>The name.</value>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the resource description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The type.</value>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Gets or sets the icon URI.</summary>
    /// <value>The icon URI.</value>
    [JsonPropertyName("icon_uri")]
    public Uri? IconUri { get; set; }

    /// <summary>Gets or sets the scopes.</summary>
    /// <value>The scopes.</value>
    [JsonPropertyName("resource_scopes")]
    public string[] Scopes { get; set; } = [];
        
    /// <summary>
    /// Gets or sets the access policy <see cref="Uri"/>.
    /// </summary>
    [JsonPropertyName("access_policy")]
    public Uri? AccessPolicy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the registration
    /// </summary>
    [JsonPropertyName("registered")]
    public long Registered { get; set; }

    /// <summary>
    /// Gets or sets the resource set id.
    /// </summary>
    [JsonPropertyName("resource_set_id")]
    public string? ResourceSetId { get; set; }

    /// <summary>
    /// Gets or sets the resource owner.
    /// </summary>
    [JsonPropertyName("owner")]
    public string Owner { get; set; } = null!;

    /// <summary>
    /// Gets or sets the source of the content
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tags for the registration.
    /// </summary>
    [JsonPropertyName("tags")]
    public string[] Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Metadata[] Metadata { get; set; } = [];

    public abstract RegistrationData ToRegistrationData();
}
