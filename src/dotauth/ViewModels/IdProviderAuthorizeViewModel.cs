namespace DotAuth.ViewModels;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Defines the abstract ID provider authorize view model.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class IdProviderAuthorizeViewModel
{
    /// <summary>
    /// Gets or sets the identifier providers.
    /// </summary>
    /// <value>
    /// The identifier providers.
    /// </value>
    public IdProviderViewModel[] IdProviders { get; set; } = [];
}