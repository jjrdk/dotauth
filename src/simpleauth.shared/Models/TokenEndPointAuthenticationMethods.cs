namespace SimpleAuth.Shared.Models
{
    using System;

    public enum TokenEndPointAuthenticationMethods
    {
        // Defaut value
        client_secret_basic = 0,
        client_secret_post = 1,
        client_secret_jwt = 2,
        private_key_jwt = 3,
        tls_client_auth = 4,
        none = 5
    }
}