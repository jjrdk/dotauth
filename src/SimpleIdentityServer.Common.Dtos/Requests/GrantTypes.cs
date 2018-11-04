namespace SimpleIdentityServer.Shared.Requests
{
    public enum GrantTypes
    {
        password,
        client_credentials,
        authorization_code,
        validate_bearer,
        refresh_token,
        uma_ticket
    }
}