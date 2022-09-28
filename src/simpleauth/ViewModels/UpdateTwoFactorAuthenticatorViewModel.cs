namespace SimpleAuth.ViewModels;

/// <summary>
/// View model to handle updates to two-factor authentication
/// </summary>
public sealed class UpdateTwoFactorAuthenticatorViewModel
{
    /// <summary>
    /// Gets or sets the type of the selected two factor authentication.
    /// </summary>
    /// <value>
    /// The type of the selected two factor authentication.
    /// </value>
    public string? SelectedTwoFactorAuthType { get; set; }
}