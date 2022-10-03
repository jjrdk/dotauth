namespace DotAuth.Shared.Responses;

internal enum AuthorizationPolicyResultKind
{
    NotAuthorized,
    NeedInfo, // default : Not supported yet
    RequestSubmitted,
    Authorized
}