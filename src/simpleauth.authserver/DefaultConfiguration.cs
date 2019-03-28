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

    public static class DefaultConfiguration
    {
        public static List<Client> GetClients()
        {
            var path = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "mycert.pfx");
            var certificate = new X509Certificate2(path, "simpleauth", X509KeyStorageFlags.Exportable);
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
                            "supersecretkey".CreateSignatureJwk(),
                            "supersecretkey".CreateEncryptionJwk()
                            
                        }.ToJwks(),
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new[] {"register_client"},
                    GrantTypes = new[] {GrantTypes.ClientCredentials, GrantTypes.Implicit, GrantTypes.AuthorizationCode},
                    ResponseTypes = new[] {ResponseTypeNames.Token, ResponseTypeNames.Code},
                    ApplicationType = ApplicationTypes.Native,
                    RedirectionUrls = new []{new Uri("http://localhost:60000"), }
                },
                new Client
                {
                    ClientId = "mobileapp",
                    ClientName = "Admin client",
                    Secrets =
                        new[]
                        {
                            new ClientSecret
                            {
                                Type = ClientSecretTypes.SharedSecret,
                                Value = "Secret"
                            }
                        },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    AllowedScopes = new[] {"openid", "profile", "role"},
                    GrantTypes = new[]
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
                        new[]
                        {
                            "supersecretkey".CreateSignatureJwk(),
                            "supersecretkey".CreateEncryptionJwk()

                        }.ToJwks(),
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Token,
                        ResponseTypeNames.IdToken
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256, // SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Native
                }
            };
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
                    CreateDateTime = DateTime.UtcNow,
                },
                new ResourceOwner
                {
                    Subject = "Doctor",
                    Password = "password".ToSha256Hash(),
                    Claims = new[]
                    {
                        new Claim(OpenIdClaimTypes.Subject, "Doctor"),
                        //new Claim(OpenIdClaimTypes.Role, "administrator"),
                        //new Claim(OpenIdClaimTypes.Role, "role"),
                        new Claim(OpenIdClaimTypes.Role, "Doctor"),
                        new Claim(OpenIdClaimTypes.Profile, "Doctor")
                    }
                }
            };
        }
    }
}
