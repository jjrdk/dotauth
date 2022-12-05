namespace DotAuth.Shared.Responses;

/// <summary>
/// Defines the authorization policy result kind.
/// </summary>
public enum AuthorizationPolicyResultKind
{
    /// <summary>
    /// Not authorized.
    /// </summary>
    NotAuthorized,
    /// <summary>
    /// Additional user info needed.
    /// </summary>
    NeedInfo, // default : Not supported yet
    /// <summary>
    /// Authorization request submitted.
    /// </summary>
    RequestSubmitted,
    /// <summary>
    /// Authorized.
    /// </summary>
    Authorized
}