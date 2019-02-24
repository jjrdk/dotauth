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

namespace SimpleAuth.Extensions
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Parameters;
    using Shared.Models;
    using Shared.Requests;
    using Shared.Responses;
    using SimpleAuth.Common;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;

    internal static class MappingExtensions
    {
        public static SearchAuthPoliciesResponse ToResponse(this GenericResult<Policy> searchAuthPoliciesResult)
        {
            if (searchAuthPoliciesResult == null)
            {
                throw new ArgumentNullException(nameof(searchAuthPoliciesResult));
            }

            return new SearchAuthPoliciesResponse
            {
                StartIndex = searchAuthPoliciesResult.StartIndex,
                TotalResults = searchAuthPoliciesResult.TotalResults,
                Content = searchAuthPoliciesResult.Content == null
                    ? Array.Empty<PolicyResponse>()
                    : searchAuthPoliciesResult.Content.Select(s => s.ToResponse()).ToArray()
            };
        }

        public static ResourceSetResponse ToResponse(this ResourceSet resourceSet)
        {
            return new ResourceSetResponse
            {
                Id = resourceSet.Id,
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = resourceSet.Scopes,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri
            };
        }

        public static PolicyResponse ToResponse(this Policy policy)
        {
            var rules = policy.Rules == null ? Array.Empty<PolicyRuleResponse>()
                : policy.Rules.Select(p => p.ToResponse()).ToArray();
            return new PolicyResponse
            {
                Id = policy.Id,
                ResourceSetIds = policy.ResourceSetIds,
                Rules = rules
            };
        }

        private static PolicyRuleResponse ToResponse(this PolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? Array.Empty<Claim>()
                : policyRule.Claims.ToArray();
            return new PolicyRuleResponse
            {
                Id = policyRule.Id,
                Claims = claims,
                ClientIdsAllowed = policyRule.ClientIdsAllowed,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Scopes = policyRule.Scopes,
                Script = policyRule.Script,
                OpenIdProvider = policyRule.OpenIdProvider
            };
        }

        public static GrantedTokenResponse ToDto(this GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }

            return new GrantedTokenResponse
            {
                AccessToken = grantedToken.AccessToken,
                IdToken = grantedToken.IdToken,
                ExpiresIn = grantedToken.ExpiresIn,
                RefreshToken = grantedToken.RefreshToken,
                TokenType = grantedToken.TokenType,
                Scope = grantedToken.Scope.Split(' ').ToArray()
            };
        }

        public static ResourceOwnerGrantTypeParameter ToResourceOwnerGrantTypeParameter(this TokenRequest request)
        {
            return new ResourceOwnerGrantTypeParameter
            {
                UserName = request.username,
                Password = request.password,
                Scope = request.scope,
                ClientId = request.client_id,
                ClientAssertion = request.client_assertion,
                ClientAssertionType = request.client_assertion_type,
                ClientSecret = request.client_secret,
                AmrValues = string.IsNullOrWhiteSpace(request.amr_values) ? Array.Empty<string>() : request.amr_values.Split(' ')
            };
        }

        public static AuthorizationCodeGrantTypeParameter ToAuthorizationCodeGrantTypeParameter(this TokenRequest request)
        {
            return new AuthorizationCodeGrantTypeParameter
            {
                ClientId = request.client_id,
                ClientSecret = request.client_secret,
                Code = request.code,
                RedirectUri = request.redirect_uri,
                ClientAssertion = request.client_assertion,
                ClientAssertionType = request.client_assertion_type,
                CodeVerifier = request.code_verifier
            };
        }

        public static RefreshTokenGrantTypeParameter ToRefreshTokenGrantTypeParameter(this TokenRequest request)
        {
            return new RefreshTokenGrantTypeParameter
            {
                RefreshToken = request.refresh_token,
                ClientAssertion = request.client_assertion,
                ClientAssertionType = request.client_assertion_type,
                ClientId = request.client_id,
                ClientSecret = request.client_secret
            };
        }

        public static ClientCredentialsGrantTypeParameter ToClientCredentialsGrantTypeParameter(this TokenRequest request)
        {
            return new ClientCredentialsGrantTypeParameter
            {
                ClientAssertion = request.client_assertion,
                ClientAssertionType = request.client_assertion_type,
                ClientId = request.client_id,
                ClientSecret = request.client_secret,
                Scope = request.scope
            };
        }

        public static GetTokenViaTicketIdParameter ToTokenIdGrantTypeParameter(this TokenRequest request)
        {
            return new GetTokenViaTicketIdParameter
            {
                ClaimToken = request.claim_token,
                ClaimTokenFormat = request.claim_token_format,
                ClientId = request.client_id,
                ClientAssertion = request.client_assertion,
                ClientAssertionType = request.client_assertion_type,
                ClientSecret = request.client_secret,
                Pct = request.pct,
                Rpt = request.rpt,
                Ticket = request.ticket
            };
        }

        public static IntrospectionParameter ToParameter(this IntrospectionRequest viewModel)
        {
            return new IntrospectionParameter
            {
                ClientAssertion = viewModel.client_assertion,
                ClientAssertionType = viewModel.client_assertion_type,
                ClientId = viewModel.client_id,
                ClientSecret = viewModel.client_secret,
                Token = viewModel.token,
                TokenTypeHint = viewModel.token_type_hint
            };
        }

        public static RevokeTokenParameter ToParameter(this RevocationRequest revocationRequest)
        {
            return new RevokeTokenParameter
            {
                ClientAssertion = revocationRequest.client_assertion,
                ClientAssertionType = revocationRequest.client_assertion_type,
                ClientId = revocationRequest.client_id,
                ClientSecret = revocationRequest.client_secret,
                Token = revocationRequest.token,
                TokenTypeHint = revocationRequest.token_type_hint
            };
        }

        public static PagedResponse<ResourceOwnerResponse> ToDto(this GenericResult<ResourceOwner> parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return new PagedResponse<ResourceOwnerResponse>
            {
                StartIndex = parameter.StartIndex,
                TotalResults = parameter.TotalResults,
                Content = parameter.Content == null ? Array.Empty<ResourceOwnerResponse>() : parameter.Content.Select(ToDto).ToArray()
            };
        }

        public static ResourceOwnerResponse ToDto(this ResourceOwner resourceOwner)
        {
            var claims = new List<KeyValuePair<string, string>>();
            if (resourceOwner.Claims != null)
            {
                claims = resourceOwner.Claims.Select(s => new KeyValuePair<string, string>(s.Type, s.Value)).ToList();
            }

            return new ResourceOwnerResponse
            {
                Login = resourceOwner.Id,
                Password = resourceOwner.Password,
                IsLocalAccount = resourceOwner.IsLocalAccount,
                Claims = claims,
                TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication,
                CreateDateTime = resourceOwner.CreateDateTime,
                UpdateDateTime = resourceOwner.UpdateDateTime
            };
        }

        public static List<ResourceOwnerResponse> ToDtos(this ICollection<ResourceOwner> resourceOwners)
        {
            return resourceOwners.Select(r => r.ToDto()).ToList();
        }

        public static AuthorizationParameter ToParameter(this AuthorizationRequest request)
        {
            var result = new AuthorizationParameter
            {
                AcrValues = request.acr_values,
                ClientId = request.client_id,
                IdTokenHint = request.id_token_hint,
                LoginHint = request.login_hint,
                MaxAge = request.max_age,
                Nonce = request.nonce,
                Prompt = request.prompt,
                RedirectUrl = request.redirect_uri,
                ResponseMode = request.response_mode ?? ResponseModes.None,
                ResponseType = request.response_type,
                Scope = request.scope,
                State = request.state,
                UiLocales = request.ui_locales,
                OriginUrl = request.origin_url,
                SessionId = request.session_id,
                AmrValues = string.IsNullOrWhiteSpace(request.amr_values) ? Array.Empty<string>() : request.amr_values.Split(' ')
            };

            if (!string.IsNullOrWhiteSpace(request.aggregate_id))
            {
                result.ProcessId = request.aggregate_id;
            }

            if (!string.IsNullOrWhiteSpace(request.claims))
            {
                var claimsParameter = new ClaimsParameter();
                result.Claims = claimsParameter;

                var obj = JObject.Parse(request.claims);
                var idToken = obj.GetValue(CoreConstants.StandardClaimParameterNames._idTokenName);
                var userInfo = obj.GetValue(CoreConstants.StandardClaimParameterNames._userInfoName);
                if (idToken != null)
                {
                    claimsParameter.IdToken = new List<ClaimParameter>();
                    FillInClaimsParameter(idToken, claimsParameter.IdToken);
                }

                if (userInfo != null)
                {
                    claimsParameter.UserInfo = new List<ClaimParameter>();
                    FillInClaimsParameter(userInfo, claimsParameter.UserInfo);
                }

                result.Claims = claimsParameter;
            }

            if (!string.IsNullOrWhiteSpace(request.code_challenge) && request.code_challenge_method != null)
            {
                result.CodeChallenge = request.code_challenge;
                result.CodeChallengeMethod = request.code_challenge_method;
            }

            return result;
        }

        public static AuthorizationRequest ToAuthorizationRequest(this JwtPayload jwsPayload)
        {
            var displayVal =
                jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.DisplayName);
            var responseMode =
                jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ResponseModeName);
            if (string.IsNullOrWhiteSpace(displayVal) || !Enum.TryParse(displayVal, out DisplayModes displayEnum))
            {
                displayEnum = DisplayModes.Page;
            }

            var result = new AuthorizationRequest
            {
                acr_values = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.AcrValuesName),
                claims = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ClaimsName),
                client_id = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ClientIdName),
                display = displayEnum,
                prompt = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.PromptName),
                id_token_hint = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.IdTokenHintName),
                max_age = long.Parse(jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.MaxAgeName)),
                nonce = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
                response_type = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                state = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.StateName),
                login_hint = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.LoginHintName),
                redirect_uri = new Uri(jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.RedirectUriName)),
                request = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.RequestName),
                request_uri = new Uri(jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.RequestUriName)),
                scope = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ScopeName),
                response_mode = string.IsNullOrWhiteSpace(responseMode) ? ResponseModes.None : responseMode,
                ui_locales = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.UiLocalesName),
            };

            return result;
        }

        private static void FillInClaimsParameter(
            JToken token,
            ICollection<ClaimParameter> claimParameters)
        {
            foreach (var child in token.Children())
            {
                var record = new ClaimParameter
                {
                    Name = ((JProperty)child).Name,
                    Parameters = new Dictionary<string, object>()
                };
                claimParameters.Add(record);

                var subChild = child.Children().FirstOrDefault();
                if (subChild == null)
                {
                    continue;
                }

                var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(subChild.ToString());
                record.Parameters = parameters;
            }
        }
    }
}