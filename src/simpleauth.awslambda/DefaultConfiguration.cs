namespace SimpleAuth.AwsLambda
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.Models;
    using SimpleAuth;

    public static class DefaultConfiguration
    {
        public static List<Client> GetClients()
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                return new List<Client>
                {
                    new Client
                    {
                        ClientId = "api",
                        ClientName = "api",
                        Secrets =
                            new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "api"}},
                        JsonWebKeys =
                            new[]
                            {
                                rsa.CreateJwk(
                                    "1",
                                    JsonWebKeyUseNames.Sig,
                                    true,
                                    KeyOperations.Sign,
                                    KeyOperations.Verify),
                                rsa.CreateJwk(
                                    "2",
                                    JsonWebKeyUseNames.Enc,
                                    true,
                                    KeyOperations.Encrypt,
                                    KeyOperations.Decrypt)
                            }.ToJwks(),
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                        PolicyUri = new Uri("http://openid.net"),
                        TosUri = new Uri("http://openid.net"),
                        AllowedScopes = new[] {"register_client"},
                        GrantTypes = new[] {GrantTypes.ClientCredentials},
                        ResponseTypes = new[] {ResponseTypeNames.Token},
                        ApplicationType = ApplicationTypes.Native
                    }
                };
            }
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
                    Password = "password",
                    IsLocalAccount = true,
                    CreateDateTime = DateTime.UtcNow
                }
            };
        }
    }
}
