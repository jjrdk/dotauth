namespace SimpleAuth.Shared.Responses
{
    public enum AuthorizationPolicyResultEnum
    {
        NotAuthorized,
        NeedInfo, // TODO : Not supported yet
        RequestSubmitted,
        Authorized
    }
}