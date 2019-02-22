﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.Server.Tests.Stores
{
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using SimpleAuth.Extensions;

    public static class DefaultStores
    {
        public static List<Consent> Consents()
        {
            return new List<Consent>()
            {
                new Consent
                {
                    Id = "1",
                    Client = new Client
                    {
                        ClientId = "authcode_client"
                    },
                    ResourceOwner = new ResourceOwner
                    {
                        Id = "administrator"
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    }
                },
                new Consent
                {
                    Id = "2",
                    Client = new Client
                    {
                        ClientId = "implicit_client"
                    },
                    ResourceOwner = new ResourceOwner
                    {
                        Id = "administrator"
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    }
                },
                new Consent
                {
                    Id = "3",
                    Client = new Client
                    {
                        ClientId = "hybrid_client"
                    },
                    ResourceOwner = new ResourceOwner
                    {
                        Id = "administrator"
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    }
                },
                new Consent
                {
                    Id = "4",
                    Client = new Client
                    {
                        ClientId = "pkce_client"
                    },
                    ResourceOwner = new ResourceOwner
                    {
                        Id = "administrator"
                    },
                    GrantedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    }
                }
            };
        }

        //public static List<JsonWebKey> JsonWebKeys(SharedContext sharedContext)
        //{
        //    var serializedRsa = string.Empty;
        //    using (var provider = new RSACryptoServiceProvider())
        //    {
        //        serializedRsa = RsaExtensions.ToXml(provider, true);
        //    }

        //    return new List<JsonWebKey>
        //    {
        //        sharedContext.SignatureKey,
        //        sharedContext.EncryptionKey
        //    };
        //}

        public static List<ResourceOwner> Users()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Id = "administrator",
                    Claims = new []
                    {
                        new Claim(OpenIdClaimTypes.Subject, "administrator"),
                        new Claim(OpenIdClaimTypes.Role, "administrator"),
                        new Claim(OpenIdClaimTypes.Address, "{ country : 'france' }")
                    },
                    Password = "password".ToSha256Hash(),
                    IsLocalAccount = true
                },
                new ResourceOwner
                {
                    Id = "user",
                    Password = "password".ToSha256Hash(),
                    Claims = new []
                    {
                        new Claim(OpenIdClaimTypes.Subject, "user")
                    },
                    IsLocalAccount = true
                },
                new ResourceOwner
                {
                    Id = "superuser",
                    Password = "password".ToSha256Hash(),
                    Claims = new []
                    {
                        new Claim(OpenIdClaimTypes.Subject, "superuser"),
                        new Claim(OpenIdClaimTypes.Role, "administrator"),
                        new Claim(OpenIdClaimTypes.Role, "role")
                    },
                    IsLocalAccount = true
                }
            };
        }

        public static List<Client> Clients(SharedContext sharedCtx)
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    ClientName = "client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "role"
                        },
                        new Scope
                        {
                            Name = "profile"
                        },
                        new Scope
                        {
                            Name = "scim"
                        },
                        new Scope
                        {
                            Name = "address"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.RefreshToken,
                        GrantTypes.Password
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Code,
                        ResponseTypeNames.Token,
                        ResponseTypeNames.IdToken
                    },
                    JsonWebKeys = new JsonWebKeySet().AddKey(TestKeys.SecretKey.CreateJwk(JsonWebKeyUseNames.Sig,
                        KeyOperations.Sign,
                        KeyOperations.Verify)),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri>
                    {
                        new Uri("https://localhost:4200/callback")
                    }
                },
                new Client
                {
                    ClientId = "client_userinfo_sig_rs256",
                    ClientName = "client_userinfo_sig_rs256",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "client_userinfo_sig_rs256"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "role"
                        },
                        new Scope
                        {
                            Name = "profile"
                        },
                        new Scope
                        {
                            Name = "scim"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.RefreshToken,
                        GrantTypes.Password
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Code,
                        ResponseTypeNames.Token,
                        ResponseTypeNames.IdToken
                    },
                    JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    UserInfoSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("https://localhost:4200/callback")}
                },
                new Client
                {
                    ClientId = "client_userinfo_enc_rsa15",
                    ClientName = "client_userinfo_enc_rsa15",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "client_userinfo_enc_rsa15"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "role"
                        },
                        new Scope
                        {
                            Name = "profile"
                        },
                        new Scope
                        {
                            Name = "scim"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.RefreshToken,
                        GrantTypes.Password
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Code,
                        ResponseTypeNames.Token,
                        ResponseTypeNames.IdToken
                    },
                    JsonWebKeys = new JsonWebKeySet().AddKey(TestKeys.SecretKey.CreateSignatureJwk())
                        .AddKey(TestKeys.SecretKey.CreateEncryptionJwk()),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    UserInfoSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    UserInfoEncryptedResponseAlg = SecurityAlgorithms.EcdsaSha256,
                    UserInfoEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("https://localhost:4200/callback")}
                },
                new Client
                {
                    ClientId = "clientWithWrongResponseType",
                    ClientName = "clientWithWrongResponseType",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "clientWithWrongResponseType"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "role"
                        },
                        new Scope
                        {
                            Name = "profile"
                        },
                        new Scope
                        {
                            Name = "scim"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.RefreshToken,
                        GrantTypes.ClientCredentials
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.IdToken
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("https://localhost:4200/callback")}
                },
                new Client
                {
                    ClientId = "clientCredentials",
                    ClientName = "clientCredentials",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "clientCredentials"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.RefreshToken,
                        GrantTypes.ClientCredentials
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Token
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("https://localhost:4200/callback")}
                },
                new Client
                {
                    ClientId = "basic_client",
                    ClientName = "basic_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "basic_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretBasic,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.ClientCredentials
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Token
                    },
                    JsonWebKeys = new JsonWebKeySet().AddKey(TestKeys.SecretKey.CreateSignatureJwk()),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("https://localhost:4200/callback")}
                },
                new Client
                {
                    ClientId = "post_client",
                    ClientName = "post_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "post_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.ClientCredentials
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Token
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("https://localhost:4200/callback")}
                },
                new Client
                {
                    ClientId = "jwt_client",
                    ClientName = "jwt_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "jwt_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretJwt,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.ClientCredentials
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Token
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("https://localhost:4200/callback")},
                    JsonWebKeys = new[]
                    {
                        sharedCtx.ModelSignatureKey,
                        sharedCtx.ModelEncryptionKey
                    }.ToJwks()
                },
                new Client
                {
                    ClientId = "private_key_client",
                    ClientName = "private_key_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "private_key_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.PrivateKeyJwt,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.ClientCredentials
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Token
                    },
                    JsonWebKeys = new JsonWebKeySet().AddKey(TestKeys.SecretKey.CreateSignatureJwk()),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256Signature, //SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("https://localhost:4200/callback")},
                    //JwksUri = new Uri("http://localhost:5000/jwks_client")
                },
                new Client
                {
                    ClientId = "authcode_client",
                    ClientName = "authcode_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "authcode_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.AuthorizationCode
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Code,
                        ResponseTypeNames.Token,
                        ResponseTypeNames.IdToken
                    },
                    JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet().AddKey(TestKeys.SuperSecretKey.CreateEncryptionJwk()),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256, //SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("http://localhost:5000/callback")}
                },
                new Client
                {
                    ClientId = "incomplete_authcode_client",
                    ClientName = "incomplete_authcode_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "incomplete_authcode_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.AuthorizationCode
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.IdToken
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("http://localhost:5000/callback")}
                },
                new Client
                {
                    ClientId = "implicit_client",
                    ClientName = "implicit_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "implicit_client"
                        }
                    },
                    JsonWebKeys = new JsonWebKeySet().AddKey(TestKeys.SecretKey.CreateSignatureJwk()),
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.Implicit
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Token,
                        ResponseTypeNames.IdToken
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("http://localhost:5000/callback")}
                },
                new Client
                {
                    ClientId = "pkce_client",
                    ClientName = "pkce_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "pkce_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.AuthorizationCode
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Code,
                        ResponseTypeNames.Token,
                        ResponseTypeNames.IdToken
                    },
                    JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("http://localhost:5000/callback")},
                    RequirePkce = true
                },
                new Client
                {
                    ClientId = "hybrid_client",
                    ClientName = "hybrid_client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "hybrid_client"
                        }
                    },
                    JsonWebKeys = new JsonWebKeySet().AddKey(TestKeys.SecretKey.CreateSignatureJwk()),
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new[]
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new[]
                    {
                        GrantTypes.AuthorizationCode,
                        GrantTypes.Implicit
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Code,
                        ResponseTypeNames.Token,
                        ResponseTypeNames.IdToken
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Web,
                    RedirectionUrls = new List<Uri> {new Uri("http://localhost:5000/callback")},
                },
                // Certificate test client.
                new Client
                {
                    ClientId = "certificate_client",
                    ClientName = "Certificate test client",
                    Secrets = new[]
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.X509Thumbprint,
                            Value = "0772F57C594FA1EFD619AF8D84A48F4C1741C715"
                        },
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.X509Name,
                            Value = "O=reimers.dk, L=Zurich, S=ZH, C=CH"
                        }
                    },
                    JsonWebKeys = new JsonWebKeySet().AddKey(
                        new X509Certificate2("mycert.pfx", "simpleauth", X509KeyStorageFlags.Exportable).CreateJwk(JsonWebKeyUseNames.Sig,
                            KeyOperations.Sign,
                            KeyOperations.Verify))
                        .AddKey(
                            new X509Certificate2("mycert.pfx", "simpleauth", X509KeyStorageFlags.Exportable).CreateJwk(JsonWebKeyUseNames.Enc,
                            KeyOperations.Encrypt,
                            KeyOperations.Decrypt)),
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.TlsClientAuth,
                    //LogoUri = null,
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.Password
                    },
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Token,
                        ResponseTypeNames.IdToken
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaV15KeyWrap, //SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Native
                },
                // Client credentials + stateless access token.
                new Client
                {
                    ClientId = "stateless_client",
                    ClientName = "Stateless client",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "stateless_client"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "register_client"
                        },
                        new Scope
                        {
                            Name = "manage_profile"
                        },
                        new Scope
                        {
                            Name = "manage_account_filtering"
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.ClientCredentials
                    },
                    JsonWebKeys = new JsonWebKeySet().AddKey(TestKeys.SecretKey.CreateJwk(JsonWebKeyUseNames.Sig,
                        KeyOperations.Sign,
                        KeyOperations.Verify)),
                    ResponseTypes = new[]
                    {
                        ResponseTypeNames.Token
                    },
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256, // SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Native
                }
            };
        }
    }
}
