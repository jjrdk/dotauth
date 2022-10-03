namespace DotAuth.ViewModels;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Defines the device authorization view model.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class DeviceAuthorizationViewModel
{
    /// <summary>
    /// Gets or sets the user code.
    /// </summary>
    public string? Code { get; set; }
}