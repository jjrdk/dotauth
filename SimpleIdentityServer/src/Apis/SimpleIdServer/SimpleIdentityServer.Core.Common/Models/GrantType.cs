namespace SimpleIdentityServer.Core.Common.Models
{
    public enum GrantType
    {
        authorization_code = 0,
        @implicit = 1,
        refresh_token = 2,
        client_credentials = 3,
        password = 4,
        uma_ticket = 5
    }
}