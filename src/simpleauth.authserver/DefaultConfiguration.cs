namespace SimpleAuth.AuthServer
{
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;

    public static class DefaultConfiguration
    {
        public static List<Client> GetClients()
        {
            //var path = Path.Combine(
            //    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            //    "mycert.pfx");
            //var certificate = new X509Certificate2(path, "simpleauth", X509KeyStorageFlags.Exportable);
            return new List<Client>
            {
                new Client
                {
                    ClientId = "api",
                    ClientName = "api",
                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "api"}},
                    JsonWebKeys =
                        new[]
                        {
                            "longsupersecretkey".CreateSignatureJwk(),
                            "longsupersecretkey".CreateEncryptionJwk()
                        }.ToJwks(),
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new[] {"register_client"},
                    GrantTypes =
                        new[] {GrantTypes.ClientCredentials, GrantTypes.Implicit, GrantTypes.AuthorizationCode},
                    ResponseTypes = new[] {ResponseTypeNames.Token, ResponseTypeNames.Code},
                    ApplicationType = ApplicationTypes.Native,
                    RedirectionUrls = new[] {new Uri("http://localhost:60000"),}
                },
                new Client
                {
                    ClientId = "mobileapp",
                    ClientName = "Admin client",
                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "Secret"}},
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    AllowedScopes = new[] {"openid", "profile", "role"},
                    GrantTypes =
                        new[]
                        {
                            GrantTypes.AuthorizationCode,
                            GrantTypes.ClientCredentials,
                            GrantTypes.Implicit,
                            GrantTypes.Password,
                            GrantTypes.RefreshToken,
                            GrantTypes.UmaTicket,
                            GrantTypes.ValidateBearer
                        },
                    JsonWebKeys =
                        new[] {"longsupersecretkey".CreateSignatureJwk(), "longsupersecretkey".CreateEncryptionJwk()}
                            .ToJwks(),
                    ResponseTypes = new[] {ResponseTypeNames.Token, ResponseTypeNames.IdToken},
                    IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256, // SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Native
                },
                new Client
                {
                    ClientId = "web",
                    ClientName = "web",
                    AllowedScopes = new[] {"openid", "role", "profile", "manager", "uma_protection"},
                    ApplicationType = ApplicationTypes.Web,
                    GrantTypes = GrantTypes.All,
                    RedirectionUrls =
                        new[]
                        {
                            new Uri("http://localhost:4200/callback"),
                            new Uri("https://localhost:5001/signin-oidc"),
                        },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    ResponseTypes =
                        new[] {ResponseTypeNames.IdToken, ResponseTypeNames.Token, ResponseTypeNames.Code},
                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
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
            return new List<Scope> { };
        }

        public static List<ResourceOwner> GetUsers()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Subject = "administrator",
                    Claims = new[]
                    {
                        new Claim(StandardClaimNames.Subject, "administrator"),
                        new Claim("role", "administrator")
                    },
                    Password = "password".ToSha256Hash(),
                    IsLocalAccount = true,
                    CreateDateTime = DateTimeOffset.UtcNow,
                }
            };
        }
    }
}
