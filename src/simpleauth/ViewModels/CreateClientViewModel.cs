namespace SimpleAuth.ViewModels;

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
}