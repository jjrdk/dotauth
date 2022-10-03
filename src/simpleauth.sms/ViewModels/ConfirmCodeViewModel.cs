namespace DotAuth.Sms.ViewModels;

/// <summary>
/// Defines the confirm code view model.
/// </summary>
public sealed class ConfirmCodeViewModel
{
    /// <summary>
    /// Gets or sets the code.
    /// </summary>
    /// <value>
    /// The code.
    /// </value>
    public string? Code { get; set; }

    /// <summary>
    /// Gets or sets the confirmation code.
    /// </summary>
    /// <value>
    /// The confirmation code.
    /// </value>
    public string? ConfirmationCode { get; set; }

    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    /// <value>
    /// The action.
    /// </value>
    public string? Action { get; set; }
}