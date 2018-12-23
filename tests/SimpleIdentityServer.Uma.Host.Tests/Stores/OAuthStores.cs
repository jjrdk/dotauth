using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Host.Tests.Stores
{
    using Shared;
    using Shared.Models;

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
            //var serializedRsa = string.Empty;
            //    using (var rsa = new RSAOpenSsl())
            //    {
            //        serializedRsa = rsa.ToXmlString(true);
            //    }
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
                        GrantTypes = new List<GrantType>
                        {
                            GrantType.client_credentials,
                            GrantType.uma_ticket
                        },
                        ResponseTypes = new List<ResponseType>
                        {
                            ResponseType.token
                        },
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.native
                    },
                    // Anonymous.
                    new Client
                    {
                        ClientId = "anonymous",
                        ClientName = "Anonymous",
                        Secrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Type = ClientSecretTypes.SharedSecret,
                                Value = "anonymous"
                            }
                        },
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = null,
                        AllowedScopes = new List<Scope> {},
                        GrantTypes = new List<GrantType>
                        {
                            GrantType.client_credentials
                        },
                        ResponseTypes = new List<ResponseType>
                        {
                            ResponseType.token
                        },
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.native
                    }

            };
        }
    }
}
