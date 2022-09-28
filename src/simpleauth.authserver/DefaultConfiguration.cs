namespace SimpleAuth.AuthServer;

using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.RegularExpressions;

public static class DefaultConfiguration
{
    public static List<Client> GetClients()
    {
        return new()
        {
            new Client
            {
                ClientId = "web",
                ClientName = "web",
                AllowedScopes = new[] {"openid", "role", "profile", "email", "manager", "uma_protection"},
                ApplicationType = ApplicationTypes.Web,
                GrantTypes = GrantTypes.All,
                RequirePkce = true,
                RedirectionUrls =
                    new[]
                    {
                        new Uri("http://localhost:4200/login"),
                        new Uri("https://localhost:50001/signin-oidc"),
                        new Uri("https://localhost:5001/signin-oidc"),
                        new Uri("https://localhost:5001/callback"),
                        new Uri("http://localhost:5000/callback"),
                        new Uri("http://localhost/callback"),
                    },
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.None,
                PostLogoutRedirectUris = new[] {new Uri("http://localhost/login")},
                ResponseTypes = ResponseTypeNames.All,
                Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                UserClaimsToIncludeInAuthToken = new[]
                {
                    new Regex($"^{OpenIdClaimTypes.Subject}$", RegexOptions.Compiled),
                    new Regex($"^{OpenIdClaimTypes.Role}$", RegexOptions.Compiled)
                },
            }
        };
    }

    public static List<Scope> GetScopes()
    {
        return new();
    }

    public static List<ResourceOwner> GetUsers(string salt)
    {
        return new()
        {
            new ResourceOwner
            {
                Subject = "administrator",
                Claims = new[]
                {
                    new Claim(StandardClaimNames.Subject, "administrator"),
                    new Claim("role", "administrator"),
                    new Claim("role", "uma_admin"),
                    new Claim(OpenIdClaimTypes.Name, "Anne Admin"),
                    new Claim(OpenIdClaimTypes.Email, "admin@server.com"),
                    new Claim(OpenIdClaimTypes.EmailVerified, bool.TrueString)
                },
                Password = "password".ToSha256Hash(salt),
                IsLocalAccount = true,
                CreateDateTime = DateTimeOffset.UtcNow,
            }
        };
    }
}