// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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
    using SimpleAuth.Extensions;

    public static class FakeOpenIdAssets
    {
        /// <summary>
        /// Get a list of fake clients
        /// </summary>
        /// <returns></returns>
        public static Client[] GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "MyBlog",
                    ClientName = "My blog",
                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "MyBlog"}},
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretBasic,
                    AllowedScopes = new[]
                    {
                        // PROTECTED API SCOPES
                        "BlogApi",
                        "BlogApi:AddArticle",
                        "openid",
                        // RO SCOPES
                        "profile",
                        "email",
                        "address",
                        "phone"
                    },
                    GrantTypes = new[] {GrantTypes.Implicit, GrantTypes.AuthorizationCode},
                    ResponseTypes = new[] {ResponseTypeNames.Token, ResponseTypeNames.Code, ResponseTypeNames.IdToken},
                    JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256, //"RS256",
                    // IdTokenEncryptedResponseAlg = "RSA1_5",
                    // IdTokenEncryptedResponseEnc = "A128CBC-HS256",
                    RedirectionUrls = new []
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
            return new()
            {
                new Scope
                {
                    Name = "BlogApi",
                    Description = "Access to the blog API",
                    Type = ScopeTypes.ProtectedApi,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "BlogApi:AddArticle",
                    Description = "Access to the add article operation",
                    Type = ScopeTypes.ProtectedApi,
                    IsDisplayedInConsent = true
                },
                new Scope
                {
                    Name = "openid",
                    IsExposed = true,
                    Type = ScopeTypes.ResourceOwner,
                    IsDisplayedInConsent = false,
                    Description = "openid"
                },
                new Scope
                {
                    Name = "profile",
                    IsExposed = true,
                    Description = "Access to the profile",
                    Claims = new[]
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
                    IsDisplayedInConsent = true,
                    Description = "Access to the email",
                    Claims = new[] {OpenIdClaimTypes.Email, OpenIdClaimTypes.EmailVerified},
                    Type = ScopeTypes.ResourceOwner
                },
                new Scope
                {
                    Name = "address",
                    IsExposed = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the address",
                    Claims = new[] {OpenIdClaimTypes.Address},
                    Type = ScopeTypes.ResourceOwner
                },
                new Scope
                {
                    Name = "phone",
                    IsExposed = true,
                    IsDisplayedInConsent = true,
                    Description = "Access to the phone",
                    Claims = new[] {OpenIdClaimTypes.PhoneNumber, OpenIdClaimTypes.PhoneNumberVerified},
                    Type = ScopeTypes.ResourceOwner
                }
            };
        }
    }
}
