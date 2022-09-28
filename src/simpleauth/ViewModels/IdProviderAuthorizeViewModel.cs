namespace SimpleAuth.ViewModels;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Defines the abstract ID provider authorize view model.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class IdProviderAuthorizeViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdProviderAuthorizeViewModel"/> class.
    /// </summary>
    protected IdProviderAuthorizeViewModel()
    {
        IdProviders = Array.Empty<IdProviderViewModel>();
    }

    /// <summary>
    /// Gets or sets the identifier providers.
    /// </summary>
    /// <value>
    /// The identifier providers.
    /// </value>
    public IdProviderViewModel[] IdProviders { get; set; }
}