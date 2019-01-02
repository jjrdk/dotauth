namespace SimpleAuth.Uma.Tests.Stores
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using System.Collections.Generic;

    public static class OAuthStores
    {
        public static List<Scope> GetScopes()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "uma_protection",
                    Description = "Access to UMA permission, resource set & token introspection endpoints",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true,
                    Type = ScopeType.ProtectedApi
                },
                new Scope
                {
                    Name = "uma_authorization",
                    Description = "Access to the UMA authorization endpoint",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true,
                    Type = ScopeType.ProtectedApi
                }
            };
        }

        public static List<JsonWebKey> GetJsonWebKeys(SharedContext sharedContext)
        {
            return new List<JsonWebKey>
            {
                sharedContext.EncryptionKey,
                sharedContext.SignatureKey
            };
        }

        public static List<Client> GetClients()
        {
            return new List<Client>
            {
                // Resource server.
                new Client
                {
                    ClientId = "resource_server",
                    ClientName = "Resource server",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "resource_server"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = null,
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "uma_protection"
                        },
                        new Scope
                        {
                            Name = "uma_authorization"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantType.client_credentials,
                        GrantType.uma_ticket
                    },
                    ResponseTypes = new []
                    {
                        ResponseTypeNames.Token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.native
                },
                // Anonymous.
                new Client
                {
                    ClientId = "anonymous",
                    ClientName = "Anonymous",
                    Secrets = new []
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "anonymous"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    LogoUri = null,
                    AllowedScopes = new List<Scope> { },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.client_credentials
                    },
                    ResponseTypes = new []
                    {
                        ResponseTypeNames.Token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.native
                }

            };
        }
    }
}
