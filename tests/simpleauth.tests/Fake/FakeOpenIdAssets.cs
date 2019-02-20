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

namespace SimpleAuth.Tests.Fake
{
    using Helpers;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;

    public static class FakeOpenIdAssets
    {
        /// <summary>
        /// Get a list of fake clients
        /// </summary>
        /// <returns></returns>
        public static Client[] GetClients()
        {
            return new []
            {
                new Client
                {
                    ClientId = "MyBlog",
                    ClientName = "My blog",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "MyBlog"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretBasic,
                    AllowedScopes = new List<Scope>
                    {
                        // PROTECTED API SCOPES
                        new Scope
                        {
                            Name = "BlogApi"
                        },
                        new Scope
                        {
                            Name = "BlogApi:AddArticle"
                        },
                        new Scope
                        {
                            Name = "openid",
                            IsExposed = true,
                            IsOpenIdScope = true,
                            Description = "openid",
                            Type = ScopeTypes.ProtectedApi
                        },
                        // RO SCOPES
                        new Scope
                        {
                            Name = "profile",
                            IsExposed = true,
                            IsOpenIdScope = true,
                            Description = "Access to the profile",
                            Claims = new []
                            {
                                OpenIdClaimTypes.Name,
                                OpenIdClaimTypes.FamilyName,
                                OpenIdClaimTypes.GivenName,
                                OpenIdClaimTypes.MiddleName,
                                OpenIdClaimTypes.NickName,
                                OpenIdClaimTypes.PreferredUserName,
                                OpenIdClaimTypes.Profile,
                                OpenIdClaimTypes.Picture,
                                OpenIdClaimTypes.WebSite,
                                OpenIdClaimTypes.Gender,
                                OpenIdClaimTypes.BirthDate,
                                OpenIdClaimTypes.ZoneInfo,
                                OpenIdClaimTypes.Locale,
                                OpenIdClaimTypes.UpdatedAt
                            },
                            Type = ScopeTypes.ResourceOwner
                        },
                        new Scope
                        {
                            Name = "email",
                            IsExposed = true,
                            IsOpenIdScope = true,
                            IsDisplayedInConsent = true,
                            Description = "Access to the email",
                            Claims = new []
                            {
                                OpenIdClaimTypes.Email,
                                OpenIdClaimTypes.EmailVerified
                            },
                            Type = ScopeTypes.ResourceOwner
                        },
                        new Scope
                        {
                            Name = "address",
                            IsExposed = true,
                            IsOpenIdScope = true,
                            IsDisplayedInConsent = true,
                            Description = "Access to the address",
                            Claims = new []
                            {
                                OpenIdClaimTypes.Address
                            },
                            Type = ScopeTypes.ResourceOwner
                        },
                        new Scope
                        {
                            Name = "phone",
                            IsExposed = true,
                            IsOpenIdScope = true,
                            IsDisplayedInConsent = true,
                            Description = "Access to the phone",
                            Claims = new []
                            {
                                OpenIdClaimTypes.PhoneNumber,
                                OpenIdClaimTypes.PhoneNumberVerified
                            },
                            Type = ScopeTypes.ResourceOwner
                        }
                    },
                    GrantTypes = new []
                    {
                        GrantTypes.Implicit,
                        GrantTypes.AuthorizationCode
                    },
                    ResponseTypes = new []
                    {
                        ResponseTypeNames.Token,
                        ResponseTypeNames.Code,
                        ResponseTypeNames.IdToken
                    },
                    JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256, //"RS256",
                    // IdTokenEncryptedResponseAlg = "RSA1_5",
                    // IdTokenEncryptedResponseEnc = "A128CBC-HS256",
                    RedirectionUrls = new List<Uri>
                    {
                        new Uri("https://op.certification.openid.net:60360/authz_cb"),
                        new Uri("http://localhost"),
                        new Uri("https://op.certification.openid.net:60186/authz_cb")
                    }
                }
            };
        }

        /// <summary>
        /// Get a list of scopes
        /// </summary>
        /// <returns></returns>
        public static List<Scope> GetScopes()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "BlogApi",
                    Description = "Access to the blog API",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "BlogApi:AddArticle",
                    Description = "Access to the add article operation",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "openid",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = false,
                    Description = "openid",
                    Type = ScopeTypes.ProtectedApi
                },
                new Scope
                {
                    Name = "profile",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    Description = "Access to the profile",
                    Claims = new []
                    {
                        OpenIdClaimTypes.Name,
                        OpenIdClaimTypes.FamilyName,
                        OpenIdClaimTypes.GivenName,
                        OpenIdClaimTypes.MiddleName,
                        OpenIdClaimTypes.NickName,
                        OpenIdClaimTypes.PreferredUserName,
                        OpenIdClaimTypes.Profile,
                        OpenIdClaimTypes.Picture,
                        OpenIdClaimTypes.WebSite,
                        OpenIdClaimTypes.Gender,
                        OpenIdClaimTypes.BirthDate,
                        OpenIdClaimTypes.ZoneInfo,
                        OpenIdClaimTypes.Locale,
                        OpenIdClaimTypes.UpdatedAt
                    },
                    Type = ScopeTypes.ResourceOwner,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "email",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the email",
                    Claims = new []
                    {
                        OpenIdClaimTypes.Email,
                        OpenIdClaimTypes.EmailVerified
                    },
                    Type = ScopeTypes.ResourceOwner
                },
                new Scope
                {
                    Name = "address",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the address",
                    Claims = new []
                    {
                        OpenIdClaimTypes.Address
                    },
                    Type = ScopeTypes.ResourceOwner
                },
                new Scope
                {
                    Name = "phone",
                    IsExposed = true,
                    IsOpenIdScope = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the phone",
                    Claims = new []
                    {
                        OpenIdClaimTypes.PhoneNumber,
                        OpenIdClaimTypes.PhoneNumberVerified
                    },
                    Type = ScopeTypes.ResourceOwner
                }
            };
        }

        //public static List<Consent> GetConsents()
        //{
        //    return new List<Consent>();
        //}

        //public static List<JsonWebKey> GetJsonWebKeys()
        //{
        //    var serializedRsa = string.Empty;
        //    using (var provider = new RSACryptoServiceProvider())
        //    {
        //        serializedRsa = provider.ToXml(true);
        //    }

        //    return new List<JsonWebKey>
        //    {
        //        new JsonWebKey
        //        {
        //            Alg = SecurityAlgorithms.RsaSha256,
        //            KeyOps = new []
        //            {
        //                KeyOperations.Sign,
        //                KeyOperations.Verify
        //            },
        //            Kid = "a3rMUgMFv9tPclLa6yF3zAkfquE",
        //            Kty = KeyType.RSA,
        //            Use = Use.Sig,
        //            SerializedKey = serializedRsa,
        //        },
        //        new JsonWebKey
        //        {
        //            Alg = SecurityAlgorithms.RsaPKCS1,
        //            KeyOps = new []
        //            {
        //                KeyOperations.Encrypt,
        //                KeyOperations.Decrypt
        //            },
        //            Kid = "3",
        //            Kty = KeyType.RSA,
        //            Use = Use.Enc,
        //            SerializedKey = serializedRsa,
        //        }
        //    };
        //}
    }
}
