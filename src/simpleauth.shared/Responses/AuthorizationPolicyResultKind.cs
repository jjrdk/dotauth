namespace SimpleAuth.Shared.Responses
{
    internal enum AuthorizationPolicyResultKind
    {
        NotAuthorized,
        NeedInfo, // default : Not supported yet
        RequestSubmitted,
        Authorized
    }
}