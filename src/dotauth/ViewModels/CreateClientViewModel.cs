namespace DotAuth.ViewModels;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Defines the create client view model.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CreateClientViewModel
{
    /// <summary>
    /// Gets or sets the name of the client.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the logo uri.
    /// </summary>
    public Uri? LogoUri { get; set; }

    /// <summary>
    /// Gets or sets the application type.
    /// </summary>
    public string? ApplicationType { get; set; }

    /// <summary>
    /// Gets or sets the redirection urls.
    /// </summary>
    public string? RedirectionUrls { get; set; }

    /// <summary>
    /// Gets or sets the grant types.
    /// </summary>
    public List<string> GrantTypes { get; set; } = new();

     /// <summary>
     /// Gets or sets the token endpoint authentication method
     /// (e.g. <c>private_key_jwt</c>, <c>client_secret_jwt</c>).
     /// </summary>
    public string? TokenEndPointAuthMethod { get; set; }

     /// <summary>
     /// Gets or sets the client's JSON Web Key Set document as a JSON string,
     /// published so the server can verify client assertions (private_key_jwt).
     /// </summary>
    public string? Jwks { get; set; }

     /// <summary>
     /// Gets or sets the URL at which the client publishes its JWKS.
     /// </summary>
    public string? JwksUri { get; set; }
}