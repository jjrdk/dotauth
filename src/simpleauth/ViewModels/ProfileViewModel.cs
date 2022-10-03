namespace DotAuth.ViewModels;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

/// <summary>
/// Defines the profile view model.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ProfileViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileViewModel"/> class.
    /// </summary>
    /// <param name="claims">The claims.</param>
    public ProfileViewModel(Claim[] claims)
    {
        Name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name")?.Value ?? "Unknown";
        GivenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName || c.Type == "given_name")?.Value
                    ?? " - ";
        FamilyName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname || c.Type == "family_name")?.Value
                     ?? " - ";
        Picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;
        LinkedIdentityProviders = new List<IdentityProviderViewModel>();
        UnlinkedIdentityProviders = new List<IdentityProviderViewModel>();
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets the name of the given.
    /// </summary>
    /// <value>
    /// The name of the given.
    /// </value>
    public string GivenName { get; }

    /// <summary>
    /// Gets the name of the family.
    /// </summary>
    /// <value>
    /// The name of the family.
    /// </value>
    public string FamilyName { get; }

    /// <summary>
    /// Gets the picture.
    /// </summary>
    /// <value>
    /// The picture.
    /// </value>
    public string? Picture { get; }

    /// <summary>
    /// Gets or sets the linked identity providers.
    /// </summary>
    /// <value>
    /// The linked identity providers.
    /// </value>
    public ICollection<IdentityProviderViewModel> LinkedIdentityProviders { get; }

    /// <summary>
    /// Gets or sets the unlinked identity providers.
    /// </summary>
    /// <value>
    /// The unlinked identity providers.
    /// </value>
    public ICollection<IdentityProviderViewModel> UnlinkedIdentityProviders { get; set; }
}