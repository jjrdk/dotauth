namespace SimpleAuth.Shared.Requests;

using System.Runtime.Serialization;

/// <summary>
/// Defines the link profile request.
/// </summary>
[DataContract]
public sealed record LinkProfileRequest
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    /// <value>
    /// The user identifier.
    /// </value>
    [DataMember(Name = "user_id")]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    /// <value>
    /// The issuer.
    /// </value>
    [DataMember(Name = "issuer")]
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="LinkProfileRequest"/> is force.
    /// </summary>
    /// <value>
    ///   <c>true</c> if force; otherwise, <c>false</c>.
    /// </value>
    [DataMember(Name = "force")]
    public bool Force { get; set; }
}