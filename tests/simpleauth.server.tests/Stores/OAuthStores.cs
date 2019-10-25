namespace SimpleAuth.Server.Tests.Stores
{
    using System;
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
                    IsDisplayedInConsent = true,
                    Type = ScopeTypes.ProtectedApi
                },
                new Scope
                {
                    Name = "uma_authorization",
                    Description = "Access to the UMA authorization endpoint",
                    IsDisplayedInConsent = true,
                    Type = ScopeTypes.ProtectedApi
                }
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
                    Secrets = new[]
                    {
                        new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "resource_server"}
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    AllowedScopes = new[] {"uma_protection", "uma_authorization"},
                    GrantTypes = new[] {GrantTypes.ClientCredentials, GrantTypes.UmaTicket},
                    ResponseTypes = new[] {ResponseTypeNames.Token},
                    JsonWebKeys = "verylongsecretkey".CreateSignatureJwk().ToSet(),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Native
                },
                // Anonymous.
                new Client
                {
                    ClientId = "anonymous",
                    ClientName = "Anonymous",
                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "anonymous"}},
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    AllowedScopes = Array.Empty<string>(),
                    GrantTypes = new[] {GrantTypes.ClientCredentials},
                    ResponseTypes = new[] {ResponseTypeNames.Token},
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Native
                }
            };
        }
    }
}
