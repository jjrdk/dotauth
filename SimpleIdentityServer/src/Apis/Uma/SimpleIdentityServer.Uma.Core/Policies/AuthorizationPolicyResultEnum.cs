namespace SimpleIdentityServer.Uma.Core.Policies
{
    public enum AuthorizationPolicyResultEnum
    {
        NotAuthorized,
        NeedInfo, // TODO : Not supported yet
        RequestSubmitted,
        Authorized
    }
}