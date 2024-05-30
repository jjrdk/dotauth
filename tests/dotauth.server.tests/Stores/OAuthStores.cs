namespace DotAuth.Server.Tests.Stores;

using System;
using System.Collections.Generic;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using Microsoft.IdentityModel.Tokens;

public static class OAuthStores
{
    public static List<Scope> GetScopes()
    {
        return
        [
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
        ];
    }

    public static List<Client> GetClients()
    {
        return
        [
            new Client
            {
                ClientId = "resource_server",
                ClientName = "Resource server",
                Secrets =
                [
                    new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "resource_server" }
                ],
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                AllowedScopes = ["uma_protection", "uma_authorization"],
                GrantTypes = [GrantTypes.ClientCredentials, GrantTypes.UmaTicket],
                ResponseTypes = [ResponseTypeNames.Token],
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                ApplicationType = ApplicationTypes.Native
            },

            new Client
            {
                ClientId = "anonymous",
                ClientName = "Anonymous",
                Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "anonymous" }],
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                AllowedScopes = [],
                GrantTypes = [GrantTypes.ClientCredentials],
                ResponseTypes = [ResponseTypeNames.Token],
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                ApplicationType = ApplicationTypes.Native
            }
        ];
    }
}
