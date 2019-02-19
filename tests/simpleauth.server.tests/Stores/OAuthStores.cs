namespace SimpleAuth.Server.Tests.Stores
{
    using System.Collections.Generic;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

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
                    Type = ScopeTypes.ProtectedApi
                },
                new Scope
                {
                    Name = "uma_authorization",
                    Description = "Access to the UMA authorization endpoint",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true,
                    Type = ScopeTypes.ProtectedApi
                }
            };
        }

        //public static List<JsonWebKey> GetJsonWebKeys(SharedUmaContext sharedContext)
        //{
        //    return new List<JsonWebKey>
        //    {
        //        sharedContext.EncryptionKey,
        //        sharedContext.SignatureKey
        //    };
        //}

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
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
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
                        GrantTypes.ClientCredentials,
                        GrantTypes.UmaTicket
                    },
                    ResponseTypes = new []
                    {
                        ResponseTypeNames.Token
                    },
                    JsonWebKeys = "verylongsecretkey".CreateSignatureJwk().ToSet(),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256, //"RS256",
                    ApplicationType = ApplicationTypes.Native
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
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    AllowedScopes = new List<Scope> { },
                    GrantTypes = new []
                    {
                        GrantTypes.ClientCredentials
                    },
                    ResponseTypes = new []
                    {
                        ResponseTypeNames.Token
                    },
                    IdTokenSignedResponseAlg = "RS256",
                    ApplicationType = ApplicationTypes.Native
                }

            };
        }
    }
}
