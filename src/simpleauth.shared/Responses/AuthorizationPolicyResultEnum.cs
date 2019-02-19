namespace SimpleAuth.Shared.Responses
{
    internal enum AuthorizationPolicyResultEnum
    {
        NotAuthorized,
        NeedInfo, // default : Not supported yet
        RequestSubmitted,
        Authorized
    }
}