namespace SimpleAuth.ViewModels;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Defines the update resource owner view model.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class UpdateResourceOwnerViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateResourceOwnerViewModel"/> class.
    /// </summary>
    /// <param name="login">The login.</param>
    /// <param name="editableClaims">The editable claims.</param>
    /// <param name="notEditableClaims">The not editable claims.</param>
    /// <param name="isLocalAccount">if set to <c>true</c> [is local account].</param>
    /// <param name="selectedTwoFactorAuthType"></param>
    /// <param name="twoFactorAuthTypes"></param>
    public UpdateResourceOwnerViewModel(
        string login,
        Dictionary<string, string> editableClaims,
        Dictionary<string, string> notEditableClaims,
        bool isLocalAccount,
        string selectedTwoFactorAuthType,
        ICollection<string> twoFactorAuthTypes)
    {
        IsLocalAccount = isLocalAccount;
        SelectedTwoFactorAuthType = selectedTwoFactorAuthType;
        Credentials = new UpdateResourceOwnerCredentialsViewModel {Login = login};
        EditableClaims = editableClaims;
        NotEditableClaims = notEditableClaims;
        TwoFactorAuthTypes = twoFactorAuthTypes;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is local account.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is local account; otherwise, <c>false</c>.
    /// </value>
    public bool IsLocalAccount { get; }

    /// <summary>
    /// Gets or sets the credentials.
    /// </summary>
    /// <value>
    /// The credentials.
    /// </value>
    public UpdateResourceOwnerCredentialsViewModel Credentials { get; }

    /// <summary>
    /// Gets or sets the editable claims.
    /// </summary>
    /// <value>
    /// The editable claims.
    /// </value>
    public Dictionary<string, string> EditableClaims { get; }

    /// <summary>
    /// Gets or sets the not editable claims.
    /// </summary>
    /// <value>
    /// The not editable claims.
    /// </value>
    public Dictionary<string, string> NotEditableClaims { get; }

    /// <summary>
    /// Gets or sets the type of the selected two factor authentication.
    /// </summary>
    /// <value>
    /// The type of the selected two factor authentication.
    /// </value>
    public string SelectedTwoFactorAuthType { get; }

    /// <summary>
    /// Gets or sets the two factor authentication types.
    /// </summary>
    /// <value>
    /// The two factor authentication types.
    /// </value>
    public ICollection<string> TwoFactorAuthTypes { get; }
}