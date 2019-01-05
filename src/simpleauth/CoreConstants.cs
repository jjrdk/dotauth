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
    using Parameters;
    using Shared;
    using Shared.Models;
    using System.Collections.Generic;

    public static class CoreConstants
    {
        public const string SESSION_ID = "session_id";
        public const string DEFAULT_AMR = "pwd";

        // Open-Id Provider Authentication Policy Extension 1.0
        public static class StandardArcParameterNames
        {
            public static string OpenIdNsPage = "openid.ns.pape";

            public static string OpenIdMaxAuthAge = "openid.pape.max_auth_age";

            public static string OpenIdAuthPolicies = "openid.pape.preferred_auth_policies";

            // Namespace for the custom Assurance Level
            public static string OpenIdCustomAuthLevel = "openid.pape.auth_level.ns";

            public static string OpenIdPreferredCustomAuthLevel = "openid.pape.preferred_auth_levels";
        }

        public static class ConfigurationNames
        {
            public const string ExpirationTimeName = "ExpirationTime";
        }

        public static class StandardAuthorizationResponseNames
        {
            public static string IdTokenName = "id_token";
            public static string AccessTokenName = "access_token";
            public static string AuthorizationCodeName = "code";
            public static string StateName = "state";
            public static string SessionState = "session_state";
        }

        // Standard authentication policies.
        // They are coming from the RFC : http://openid.net/specs/openid-provider-authentication-policy-extension-1_0.html
        public static class StandardAuthenticationPolicies
        {
            public static string OpenIdPhishingResistant = "http://schemas.openid.net/pape/policies/2007/06/phishing-resistant";

            // provides more than one authentication factor for example password + software token
            public static string OpenIdMultiFactorAuth = "http://schemas.openid.net/pape/policies/2007/06/multi-factor";

            // provides more than one authentication factor with at least one physical factor
            public static string OpenIdPhysicalMultiFactorAuth = "http://schemas.openid.net/pape/policies/2007/06/multi-factor-physical";
        }

        // Standard scopes defined by OPEN-ID
        public static class StandardScopes
        {
            public static Scope ProfileScope = new Scope
            {
                Name = "profile",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the profile",
                Claims = new[]
                {
                    JwtConstants.StandardResourceOwnerClaimNames.Name,
                    JwtConstants.StandardResourceOwnerClaimNames.FamilyName,
                    JwtConstants.StandardResourceOwnerClaimNames.GivenName,
                    JwtConstants.StandardResourceOwnerClaimNames.MiddleName,
                    JwtConstants.StandardResourceOwnerClaimNames.NickName,
                    JwtConstants.StandardResourceOwnerClaimNames.PreferredUserName,
                    JwtConstants.StandardResourceOwnerClaimNames.Profile,
                    JwtConstants.StandardResourceOwnerClaimNames.Picture,
                    JwtConstants.StandardResourceOwnerClaimNames.WebSite,
                    JwtConstants.StandardResourceOwnerClaimNames.Gender,
                    JwtConstants.StandardResourceOwnerClaimNames.BirthDate,
                    JwtConstants.StandardResourceOwnerClaimNames.ZoneInfo,
                    JwtConstants.StandardResourceOwnerClaimNames.Locale,
                    JwtConstants.StandardResourceOwnerClaimNames.UpdatedAt
                },
                Type = ScopeType.ResourceOwner
            };

            public static Scope Email = new Scope
            {
                Name = "email",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the email",
                Claims = new[]
                {
                    JwtConstants.StandardResourceOwnerClaimNames.Email,
                    JwtConstants.StandardResourceOwnerClaimNames.EmailVerified
                },
                Type = ScopeType.ResourceOwner
            };

            public static Scope Address = new Scope
            {
                Name = "address",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the address",
                Claims = new[]
                {
                    JwtConstants.StandardResourceOwnerClaimNames.Address
                },
                Type = ScopeType.ResourceOwner
            };

            public static Scope Phone = new Scope
            {
                Name = "phone",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the phone",
                Claims = new[]
                {
                    JwtConstants.StandardResourceOwnerClaimNames.PhoneNumber,
                    JwtConstants.StandardResourceOwnerClaimNames.PhoneNumberVerified
                },
                Type = ScopeType.ResourceOwner
            };

            public static Scope OpenId = new Scope
            {
                Name = "openid",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = false,
                Description = "openid",
                Type = ScopeType.ProtectedApi
            };
        }

        // Defines the Assurance Level
        // For more information check this documentation : http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf
        public enum StandardNistAssuranceLevel
        {
            Level1 = 1,
            Level2 = 2,
            Level3 = 3,
            Level4 = 4
        }

        public static class StandardTokenTypes
        {
            public static string Bearer = "Bearer";
        }

        public static class StandardClaimParameterValueNames
        {
            public const string ValueName = "value";

            public const string ValuesName = "values";

            public const string EssentialName = "essential";
        }

        public static class StandardClaimParameterNames
        {
            public static string UserInfoName = "userinfo";

            public static string IdTokenName = "id_token";
        }

        public static class StandardTokenTypeHintNames
        {
            public const string AccessToken = "access_token";
            public const string RefreshToken = "refresh_token";
        }

        public static string[] AllStandardTokenTypeHintNames = new[]
        {
            StandardTokenTypeHintNames.AccessToken,
            StandardTokenTypeHintNames.RefreshToken
        };

        /// <summary>
        /// Parameter names of an authorization request
        /// </summary>
        public static class StandardAuthorizationRequestParameterNames
        {
            public static string ScopeName = "scope";
            public static string ResponseTypeName = "response_type";
            public static string ClientIdName = "client_id";
            public static string RedirectUriName = "redirect_uri";
            public static string StateName = "state";
            public static string ResponseModeName = "response_mode";
            public static string NonceName = "nonce";
            public static string DisplayName = "display";
            public static string PromptName = "prompt";
            public static string MaxAgeName = "max_age";
            public static string UiLocalesName = "ui_locales";
            public static string IdTokenHintName = "id_token_hint";
            public static string LoginHintName = "login_hint";
            public static string ClaimsName = "claims";
            public static string AcrValuesName = "acr_values";
            public static string RequestName = "request";
            public static string RequestUriName = "request_uri";
        }

        public static class IntrospectionRequestNames
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
        public static class StandardTokenRequestParameterNames
        {
            public static string ClientIdName = "client_id";
            public static string UserName = "username";
            public static string PasswordName = "password";
            public static string AuthorizationCodeName = "code";
            public static string RefreshToken = "refresh_token";
            public static string ScopeName = "scope";
        }

        public static class StandardTranslationCodes
        {
            public const string ApplicationWouldLikeToCode = "application_would_like_to";
            public const string ScopesCode = "scopes";
            public const string IndividualClaimsCode = "individual_claims";
            public const string NameCode = "Name";
            public const string LoginExternalAccount = "login_external_account";
            public const string LoginLocalAccount = "login_local_account";
            public const string UserNameCode = "username";
            public const string PasswordCode = "password";
            public const string LoginCode = "login";
            public const string RememberMyLoginCode = "remember_my_login";
            public const string CancelCode = "cancel";
            public const string ConfirmCode = "confirm";
            public const string LinkToThePolicy = "policy";
            public const string Tos = "tos";
            public const string SendCode = "send_code";
            public const string Code = "code";
            public const string EditResourceOwner = "edit_resource_owner";
            public const string YourName = "your_name";
            public const string YourPassword = "your_password";
            public const string Email = "email";
            public const string YourEmail = "your_email";
            public const string TwoAuthenticationFactor = "two_authentication_factor";
            public const string UserIsUpdated = "user_is_updated";
            public const string SendConfirmationCode = "send_confirmation_code";
            public const string Phone = "phone";
            public const string HashedPassword = "hashed_password";
            public const string CreateResourceOwner = "create_resource_owner";
            public const string Credentials = "credentials";
            public const string RepeatPassword = "repeat_password";
            public const string Claims = "claims";
            public const string UserIsCreated = "user_is_created";
            public const string TwoFactor = "two_factor";
            public const string UpdateClaim = "update_claim";
            public const string ConfirmationCode = "confirmation_code";
            public const string ResetConfirmationCode = "resend_confirmation_code";
            public const string ValidateConfirmationCode = "validate_confirmation_code";
            public const string NoTwoFactorAuthenticator = "no_two_factor_authenticator";
            public const string NoTwoFactorAuthenticatorSelected = "no_two_factor_authenticator_selected";
        }

        public static readonly Dictionary<string[], AuthorizationFlow> MappingResponseTypesToAuthorizationFlows =
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

        public static Dictionary<AuthorizationFlow, ResponseMode> MappingAuthorizationFlowAndResponseModes = new Dictionary<AuthorizationFlow, ResponseMode>
        {
            {
                AuthorizationFlow.AuthorizationCodeFlow, ResponseMode.query
            },
            {
                AuthorizationFlow.ImplicitFlow, ResponseMode.fragment
            },
            {
                AuthorizationFlow.HybridFlow, ResponseMode.fragment
            }
        };

        public static class SubjectTypeNames
        {
            public const string Public = "public";
            public const string PairWise = "pairwise";
        }

        public static class Supported
        {
            public static List<AuthorizationFlow> SupportedAuthorizationFlows = new List<AuthorizationFlow>
            {
                AuthorizationFlow.AuthorizationCodeFlow,
                AuthorizationFlow.ImplicitFlow,
                AuthorizationFlow.HybridFlow
            };

            public static List<GrantType> SupportedGrantTypes = new List<GrantType>
            {
                GrantType.authorization_code,
                GrantType.client_credentials,
                GrantType.password,
                GrantType.refresh_token,
                GrantType.@implicit
            };

            public static string[] SupportedResponseModes = new[]
            {
                "query"
            };

            public static string[] SupportedSubjectTypes = new[]
            {
                // Same subject value to all clients.
                SubjectTypeNames.Public,
                SubjectTypeNames.PairWise
            };

            //public static string[] SupportedJwsAlgs = new[]
            //{
            //    SecurityAlgorithms.RsaSha256 //JwtConstants.JwsAlgNames.RS256
            //};

            //public static string[] SupportedJweAlgs = new[]
            //{
            //    SecurityAlgorithms.RsaPKCS1
            //};

            //public static string[] SupportedJweEncs = new[]
            //{
            //    SecurityAlgorithms.Aes128CbcHmacSha256
            //};

            public static List<TokenEndPointAuthenticationMethods> SupportedTokenEndPointAuthenticationMethods =
                new List<TokenEndPointAuthenticationMethods>
            {
                TokenEndPointAuthenticationMethods.client_secret_basic,
                TokenEndPointAuthenticationMethods.client_secret_post,
                TokenEndPointAuthenticationMethods.client_secret_jwt,
                TokenEndPointAuthenticationMethods.private_key_jwt,
                TokenEndPointAuthenticationMethods.tls_client_auth
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