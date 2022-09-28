namespace SimpleAuth.Shared.Requests;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Defines the revoke session request.
/// </summary>
[DataContract]
public sealed record RevokeSessionRequest
{
    /// <summary>
    /// Gets or sets the identifier token hint.
    /// </summary>
    /// <value>
    /// The identifier token hint.
    /// </value>
    [DataMember(Name = "id_token_hint")]
#pragma warning disable IDE1006 // Naming Styles
    public string? id_token_hint { get; set; }

    /// <summary>
    /// Gets or sets the post logout redirect URI.
    /// </summary>
    /// <value>
    /// The post logout redirect URI.
    /// </value>
    [DataMember(Name = "post_logout_redirect_uri")]
    public Uri? post_logout_redirect_uri { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    /// <value>
    /// The state.
    /// </value>
    [DataMember(Name = "state")]
    public string? state { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}