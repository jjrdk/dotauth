namespace DotAuth.Sms.ViewModels;

using System.ComponentModel.DataAnnotations;
using DotAuth.ViewModels;

/// <summary>
/// Defines the SMS OpenID local authentication view model.
/// </summary>
/// <seealso cref="AuthorizeOpenIdViewModel" />
public sealed class SmsOpenIdLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
{
    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    /// <value>
    /// The phone number.
    /// </value>
    [Required]
    public string? PhoneNumber { get; set; }
}