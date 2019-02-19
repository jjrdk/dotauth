namespace SimpleAuth.Shared.Responses
{
    public enum AuthorizationPolicyResultEnum
    {
        NotAuthorized,
        NeedInfo, // default : Not supported yet
        RequestSubmitted,
        Authorized
    }
}