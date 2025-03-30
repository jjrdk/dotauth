namespace DotAuth.Stores.Marten.Containers;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the storage container type for <see cref="Scope"/>.
/// </summary>
public sealed record ScopeContainer : Scope
{
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Creates a new <see cref="ScopeContainer"/> from the specified <paramref name="scope"/>.
    /// </summary>
    /// <param name="scope">The <paramref name="scope"/> to copy content from.</param>
    /// <returns></returns>
    public static ScopeContainer Create(Scope scope)
    {
        return new ScopeContainer
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = scope.Name,
            Description = scope.Description,
            IconUri = scope.IconUri,
            IsDisplayedInConsent = scope.IsDisplayedInConsent,
            IsExposed = scope.IsExposed,
            Claims = scope.Claims,
            Type = scope.Type
        };
    }

    /// <summary>
    /// Converts the contents of this container to a <see cref="Scope"/>.
    /// </summary>
    /// <returns>A <see cref="Scope"/> instance.</returns>
    public Scope ToScope()
    {
        return new Scope
        {
            Name = Name,
            Description = Description,
            IconUri = IconUri,
            IsDisplayedInConsent = IsDisplayedInConsent,
            IsExposed = IsExposed,
            Claims = Claims,
            Type = Type
        };
    }
}
