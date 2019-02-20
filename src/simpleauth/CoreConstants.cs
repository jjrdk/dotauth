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

namespace SimpleAuth
{
    using Api.Authorization;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using System.Collections.Generic;

    internal static class CoreConstants
    {
        public const string SessionId = "session_id";
        public const string DefaultAmr = "pwd";

        // Open-ClientId Provider Authentication Policy Extension 1.0
        public static class StandardArcParameterNames
        {
            public static string _openIdNsPage = "openid.ns.pape";

            public static string _openIdMaxAuthAge = "openid.pape.max_auth_age";

            public static string _openIdAuthPolicies = "openid.pape.preferred_auth_policies";

            // Namespace for the custom Assurance Level
            public static string _openIdCustomAuthLevel = "openid.pape.auth_level.ns";

            public static string _openIdPreferredCustomAuthLevel = "openid.pape.preferred_auth_levels";
        }

        public static class StandardAuthorizationResponseNames
        {
            public static string _idTokenName = "id_token";
            public static string _accessTokenName = "access_token";
            public static string _authorizationCodeName = "code";
            public static string _stateName = "state";
            public static string _sessionState = "session_state";
        }

        //// Standard authentication policies.
        //// They are coming from the RFC : http://openid.net/specs/openid-provider-authentication-policy-extension-1_0.html
        //public static class StandardAuthenticationPolicies
        //{
        //    public static string OpenIdPhishingResistant = "http://schemas.openid.net/pape/policies/2007/06/phishing-resistant";

        //    // provides more than one authentication factor for example password + software token
        //    public static string OpenIdMultiFactorAuth = "http://schemas.openid.net/pape/policies/2007/06/multi-factor";

        //    // provides more than one authentication factor with at least one physical factor
        //    public static string OpenIdPhysicalMultiFactorAuth = "http://schemas.openid.net/pape/policies/2007/06/multi-factor-physical";
        //}

        //// Standard scopes defined by OPEN-ID
        //internal static class StandardScopes
        //{
        //    public static Scope ProfileScope = new Scope
        //    {
        //        Name = "profile",
        //        IsExposed = true,
        //        IsOpenIdScope = true,
        //        IsDisplayedInConsent = true,
        //        Description = "Access to the profile",
        //        Claims = new[]
        //        {
        //            OpenIdClaimTypes.Name,
        //            OpenIdClaimTypes.FamilyName,
        //            OpenIdClaimTypes.GivenName,
        //            OpenIdClaimTypes.MiddleName,
        //            OpenIdClaimTypes.NickName,
        //            OpenIdClaimTypes.PreferredUserName,
        //            OpenIdClaimTypes.Profile,
        //            OpenIdClaimTypes.Picture,
        //            OpenIdClaimTypes.WebSite,
        //            OpenIdClaimTypes.Gender,
        //            OpenIdClaimTypes.BirthDate,
        //            OpenIdClaimTypes.ZoneInfo,
        //            OpenIdClaimTypes.Locale,
        //            OpenIdClaimTypes.UpdatedAt
        //        },
        //        Type = ScopeTypes.ResourceOwner
        //    };

        //    public static Scope Email = new Scope
        //    {
        //        Name = "email",
        //        IsExposed = true,
        //        IsOpenIdScope = true,
        //        IsDisplayedInConsent = true,
        //        Description = "Access to the email",
        //        Claims = new[]
        //        {
        //            OpenIdClaimTypes.Email,
        //            OpenIdClaimTypes.EmailVerified
        //        },
        //        Type = ScopeTypes.ResourceOwner
        //    };

        //    public static Scope Address = new Scope
        //    {
        //        Name = "address",
        //        IsExposed = true,
        //        IsOpenIdScope = true,
        //        IsDisplayedInConsent = true,
        //        Description = "Access to the address",
        //        Claims = new[]
        //        {
        //            OpenIdClaimTypes.Address
        //        },
        //        Type = ScopeTypes.ResourceOwner
        //    };

        //    public static Scope Phone = new Scope
        //    {
        //        Name = "phone",
        //        IsExposed = true,
        //        IsOpenIdScope = true,
        //        IsDisplayedInConsent = true,
        //        Description = "Access to the phone",
        //        Claims = new[]
        //        {
        //            OpenIdClaimTypes.PhoneNumber,
        //            OpenIdClaimTypes.PhoneNumberVerified
        //        },
        //        Type = ScopeTypes.ResourceOwner
        //    };

        //    public static Scope OpenId = new Scope
        //    {
        //        Name = "openid",
        //        IsExposed = true,
        //        IsOpenIdScope = true,
        //        IsDisplayedInConsent = false,
        //        Description = "openid",
        //        Type = ScopeTypes.ProtectedApi
        //    };
        //}

        //// Defines the Assurance Level
        //// For more information check this documentation : http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf
        //public enum StandardNistAssuranceLevel
        //{
        //    Level1 = 1,
        //    Level2 = 2,
        //    Level3 = 3,
        //    Level4 = 4
        //}

        public static class StandardTokenTypes
        {
            public static string _bearer = "Bearer";
        }

        public static class StandardClaimParameterValueNames
        {
            public const string ValueName = "value";

            public const string ValuesName = "values";

            public const string EssentialName = "essential";
        }

        public static class StandardClaimParameterNames
        {
            public static string _userInfoName = "userinfo";

            public static string _idTokenName = "id_token";
        }

        public static class StandardTokenTypeHintNames
        {
            public const string AccessToken = "access_token";
            public const string RefreshToken = "refresh_token";
        }

        public static readonly string[] AllStandardTokenTypeHintNames = new[]
        {
            StandardTokenTypeHintNames.AccessToken,
            StandardTokenTypeHintNames.RefreshToken
        };

        /// <summary>
        /// Parameter names of an authorization request
        /// </summary>
        internal static class StandardAuthorizationRequestParameterNames
        {
            public const string ScopeName = "scope";
            public const string ResponseTypeName = "response_type";
            public const string ClientIdName = "client_id";
            public const string RedirectUriName = "redirect_uri";
            public const string StateName = "state";
            public const string ResponseModeName = "response_mode";
            public const string NonceName = "nonce";
            public const string DisplayName = "display";
            public const string PromptName = "prompt";
            public const string MaxAgeName = "max_age";
            public const string UiLocalesName = "ui_locales";
            public const string IdTokenHintName = "id_token_hint";
            public const string LoginHintName = "login_hint";
            public const string ClaimsName = "claims";
            public const string AcrValuesName = "acr_values";
            public const string RequestName = "request";
            public const string RequestUriName = "request_uri";
        }

        internal static class IntrospectionRequestNames
        {
            public const string Token = "token";
            public const string TokenTypeHint = "token_type_hint";
            public const string ClientId = "client_id";
            public const string ClientSecret = "client_secret";
            public const string ClientAssertion = "client_assertion";
            public const string ClientAssertionType = "client_assertion_type";
        }

        /// <summary>
        /// Parameter names of a token request
        /// </summary>
        internal static class StandardTokenRequestParameterNames
        {
            public static string _clientIdName = "client_id";
            public static string _userName = "username";
            public static string _passwordName = "password";
            public static string _authorizationCodeName = "code";
            public static string _refreshToken = "refresh_token";
            public static string _scopeName = "scope";
        }

        internal static readonly Dictionary<string[], AuthorizationFlow> MappingResponseTypesToAuthorizationFlows =
            new Dictionary<string[], AuthorizationFlow>
            {
                {
                    new[]
                    {
                        ResponseTypeNames.Code
                    },
                    AuthorizationFlow.AuthorizationCodeFlow
                },
                {
                    new[]
                    {
                        ResponseTypeNames.IdToken
                    },
                    AuthorizationFlow.ImplicitFlow
                },
                {
                    new[]
                    {
                        ResponseTypeNames.IdToken,
                        ResponseTypeNames.Token
                    },
                    AuthorizationFlow.ImplicitFlow
                },
                {
                    new[]
                    {
                        ResponseTypeNames.Code,
                        ResponseTypeNames.IdToken
                    },
                    AuthorizationFlow.HybridFlow
                },
                {
                    new[]
                    {
                        ResponseTypeNames.Code,
                        ResponseTypeNames.Token
                    },
                    AuthorizationFlow.HybridFlow
                },
                {
                    new[]
                    {
                        ResponseTypeNames.Code,
                        ResponseTypeNames.IdToken,
                        ResponseTypeNames.Token
                    },
                    AuthorizationFlow.HybridFlow
                }
            };

        internal static readonly Dictionary<AuthorizationFlow, string> MappingAuthorizationFlowAndResponseModes = new Dictionary<AuthorizationFlow, string>
        {
            {
                AuthorizationFlow.AuthorizationCodeFlow, ResponseModes.Query
            },
            {
                AuthorizationFlow.ImplicitFlow, ResponseModes.Fragment
            },
            {
                AuthorizationFlow.HybridFlow, ResponseModes.Fragment
            }
        };

        private static class SubjectTypeNames
        {
            public const string Public = "public";
            public const string PairWise = "pairwise";
        }

        internal static class Supported
        {
            public static List<AuthorizationFlow> _supportedAuthorizationFlows = new List<AuthorizationFlow>
            {
                AuthorizationFlow.AuthorizationCodeFlow,
                AuthorizationFlow.ImplicitFlow,
                AuthorizationFlow.HybridFlow
            };

            public static string[] _supportedGrantTypes = new[]
            {
                GrantTypes.AuthorizationCode,
                GrantTypes.ClientCredentials,
                GrantTypes.Password,
                GrantTypes.RefreshToken,
                GrantTypes.Implicit
            };

            public static string[] _supportedResponseModes = new[]
            {
                "query"
            };

            public static string[] _supportedSubjectTypes = new[]
            {
                // Same subject value to all clients.
                SubjectTypeNames.Public,
                SubjectTypeNames.PairWise
            };

            public static readonly List<string> SupportedTokenEndPointAuthenticationMethods =
                new List<string>
            {
                TokenEndPointAuthenticationMethods.ClientSecretBasic,
                TokenEndPointAuthenticationMethods.ClientSecretPost,
                TokenEndPointAuthenticationMethods.ClientSecretJwt,
                TokenEndPointAuthenticationMethods.PrivateKeyJwt,
                TokenEndPointAuthenticationMethods.TlsClientAuth
            };
        }

        public static class EndPoints
        {
            public const string Jws = "jws";
            public const string Jwe = "jwe";
            public const string Clients = "clients";
            public const string Scopes = "scopes";
            public const string ResourceOwners = "resource_owners";
            public const string Manage = "manage";
            public const string Claims = "claims";
            public const string DiscoveryAction = ".well-known/openid-configuration";
            public const string Authorization = "authorization";
            public const string Token = "token";
            public const string UserInfo = "userinfo";
            public const string Jwks = "jwks";
            public const string Registration = "registration";
            public const string RevokeSessionCallback = "revoke_session_callback";
            public const string Revocation = "token/revoke";
            public const string Introspection = "introspect";
            public const string CheckSession = "check_session";
            public const string EndSession = "end_session";
            public const string EndSessionCallback = "end_session_callback";
            public const string Get401 = "error/401";
            public const string Get404 = "error/404";
            public const string Get500 = "error/500";
        }
    }
}