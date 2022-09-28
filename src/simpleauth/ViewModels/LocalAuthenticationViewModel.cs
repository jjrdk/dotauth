namespace SimpleAuth.ViewModels;

/// <summary>
/// Defines the local authentication view model
/// </summary>
public sealed class LocalAuthenticationViewModel
{
    /// <summary>
    /// Gets or sets the login.
    /// </summary>
    /// <value>
    /// The login.
    /// </value>
    public string? Login { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    /// <value>
    /// The password.
    /// </value>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the optional return url.
    /// </summary>
    public string? ReturnUrl { get; set; }
}