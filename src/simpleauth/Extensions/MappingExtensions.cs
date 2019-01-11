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

namespace SimpleAuth.Server.Extensions
{
    using Microsoft.AspNetCore.Authentication;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Parameters;
    using Results;
    using Shared.Models;
    using Shared.Parameters;
    using Shared.Requests;
    using Shared.Responses;
    using Shared.Results;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using Shared.DTOs;
    using CodeChallengeMethods = Shared.Models.CodeChallengeMethods;
    using ScopeResponse = Shared.Responses.ScopeResponse;

    public static class MappingExtensions
    {
        public static SearchResourceSetParameter ToParameter(this SearchResourceSet searchResourceSet)
        {
            if (searchResourceSet == null)
            {
                throw new ArgumentNullException(nameof(searchResourceSet));
            }

            return new SearchResourceSetParameter
            {
                Count = searchResourceSet.TotalResults,
                Ids = searchResourceSet.Ids,
                Names = searchResourceSet.Names,
                StartIndex = searchResourceSet.StartIndex,
                Types = searchResourceSet.Types
            };
        }

        public static SearchAuthPoliciesParameter ToParameter(this SearchAuthPolicies searchAuthPolicies)
        {
            if (searchAuthPolicies == null)
            {
                throw new ArgumentNullException(nameof(searchAuthPolicies));
            }

            return new SearchAuthPoliciesParameter
            {
                Count = searchAuthPolicies.TotalResults,
                Ids = searchAuthPolicies.Ids,
                StartIndex = searchAuthPolicies.StartIndex,
                ResourceIds = searchAuthPolicies.ResourceIds
            };
        }

        public static AddResouceSetParameter ToParameter(this PostResourceSet postResourceSet)
        {
            return new AddResouceSetParameter
            {
                IconUri = postResourceSet.IconUri,
                Name = postResourceSet.Name,
                Scopes = postResourceSet.Scopes,
                Type = postResourceSet.Type,
                Uri = postResourceSet.Uri
            };
        }

        public static UpdateResourceSetParameter ToParameter(this PutResourceSet putResourceSet)
        {
            return new UpdateResourceSetParameter
            {
                Id = putResourceSet.Id,
                Name = putResourceSet.Name,
                IconUri = putResourceSet.IconUri,
                Scopes = putResourceSet.Scopes,
                Type = putResourceSet.Type,
                Uri = putResourceSet.Uri
            };
        }

        public static AddPermissionParameter ToParameter(this PostPermission postPermission)
        {
            return new AddPermissionParameter
            {
                ResourceSetId = postPermission.ResourceSetId,
                Scopes = postPermission.Scopes
            };
        }

        public static AddPolicyParameter ToParameter(this PostPolicy postPolicy)
        {
            var rules = postPolicy.Rules == null ? new List<AddPolicyRuleParameter>()
                : postPolicy.Rules.Select(r => r.ToParameter()).ToList();
            return new AddPolicyParameter
            {
                Rules = rules,
                ResourceSetIds = postPolicy.ResourceSetIds
            };
        }

        public static AddPolicyRuleParameter ToParameter(this PostPolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? new List<AddClaimParameter>()
                : policyRule.Claims.Select(p => p.ToParameter()).ToList();
            return new AddPolicyRuleParameter
            {
                Claims = claims,
                ClientIdsAllowed = policyRule.ClientIdsAllowed,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Scopes = policyRule.Scopes,
                Script = policyRule.Script,
                OpenIdProvider = policyRule.OpenIdProvider
            };
        }

        public static AddClaimParameter ToParameter(this PostClaim postClaim)
        {
            return new AddClaimParameter
            {
                Type = postClaim.Type,
                Value = postClaim.Value
            };
        }

        public static UpdatePolicyParameter ToParameter(this PutPolicy putPolicy)
        {
            var rules = putPolicy.Rules == null ? new List<UpdatePolicyRuleParameter>()
                : putPolicy.Rules.Select(r => r.ToParameter()).ToList();
            return new UpdatePolicyParameter
            {
                PolicyId = putPolicy.PolicyId,
                Rules = rules
            };
        }

        public static UpdatePolicyRuleParameter ToParameter(this PutPolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? new List<AddClaimParameter>()
                : policyRule.Claims.Select(p => p.ToParameter()).ToList();
            return new UpdatePolicyRuleParameter
            {
                Claims = claims,
                ClientIdsAllowed = policyRule.ClientIdsAllowed,
                Id = policyRule.Id,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Scopes = policyRule.Scopes,
                Script = policyRule.Script,
                OpenIdProvider = policyRule.OpenIdProvider
            };
        }

        public static SearchResourceSetResponse ToResponse(this SearchResourceSetResult searchResourceSetResult)
        {
            if (searchResourceSetResult == null)
            {
                throw new ArgumentNullException(nameof(searchResourceSetResult));
            }

            return new SearchResourceSetResponse
            {
                StartIndex = searchResourceSetResult.StartIndex,
                TotalResults = searchResourceSetResult.TotalResults,
                Content = searchResourceSetResult.Content == null ? new List<ResourceSetResponse>() : searchResourceSetResult.Content.Select(s => s.ToResponse())
            };
        }

        public static SearchAuthPoliciesResponse ToResponse(this SearchAuthPoliciesResult searchAuthPoliciesResult)
        {
            if (searchAuthPoliciesResult == null)
            {
                throw new ArgumentNullException(nameof(searchAuthPoliciesResult));
            }

            return new SearchAuthPoliciesResponse
            {
                StartIndex = searchAuthPoliciesResult.StartIndex,
                TotalResults = searchAuthPoliciesResult.TotalResults,
                Content = searchAuthPoliciesResult.Content == null ? new List<PolicyResponse>() : searchAuthPoliciesResult.Content.Select(s => s.ToResponse())
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
            var rules = policy.Rules == null ? new List<PolicyRuleResponse>()
                : policy.Rules.Select(p => p.ToResponse()).ToList();
            return new PolicyResponse
            {
                Id = policy.Id,
                ResourceSetIds = policy.ResourceSetIds,
                Rules = rules
            };
        }

        public static PolicyRuleResponse ToResponse(this PolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? new List<Claim>()
                : policyRule.Claims.ToList();
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

        public static UmaConfigurationResponse ToResponse(this UmaConfigurationResponse configuration)
        {
            return new UmaConfigurationResponse
            {
                ClaimTokenProfilesSupported = configuration.ClaimTokenProfilesSupported,
                IntrospectionEndpoint = configuration.IntrospectionEndpoint,
                Issuer = configuration.Issuer,
                PermissionEndpoint = configuration.PermissionEndpoint,
                AuthorizationEndpoint = configuration.AuthorizationEndpoint,
                ClaimsInteractionEndpoint = configuration.ClaimsInteractionEndpoint,
                GrantTypesSupported = configuration.GrantTypesSupported,
                JwksUri = configuration.JwksUri,
                RegistrationEndpoint = configuration.RegistrationEndpoint,
                ResourceRegistrationEndpoint = configuration.ResourceRegistrationEndpoint,
                ResponseTypesSupported = configuration.ResponseTypesSupported,
                RevocationEndpoint = configuration.RevocationEndpoint,
                PoliciesEndpoint = configuration.PoliciesEndpoint,
                ScopesSupported = configuration.ScopesSupported,
                TokenEndpoint = configuration.TokenEndpoint,
                TokenEndpointAuthMethodsSupported = configuration.TokenEndpointAuthMethodsSupported,
                TokenEndpointAuthSigningAlgValuesSupported = configuration.TokenEndpointAuthSigningAlgValuesSupported,
                UiLocalesSupported = configuration.UiLocalesSupported,
                UmaProfilesSupported = configuration.UmaProfilesSupported
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
                Scope = grantedToken.Scope.Split(' ').ToList()
            };
        }

        public static IntrospectionResponse ToDto(this IntrospectionResult introspectionResult)
        {
            return new IntrospectionResponse
            {
                Active = introspectionResult.Active,
                Audience = introspectionResult.Audience,
                ClientId = introspectionResult.ClientId,
                Expiration = introspectionResult.Expiration,
                IssuedAt = introspectionResult.IssuedAt,
                Issuer = introspectionResult.Issuer,
                Jti = introspectionResult.Jti,
                Nbf = introspectionResult.Nbf,
                Scope = introspectionResult.Scope.Split(' ').ToList(),
                Subject = introspectionResult.Subject,
                TokenType = introspectionResult.TokenType,
                UserName = introspectionResult.UserName
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
                AmrValues = string.IsNullOrWhiteSpace(request.amr_values) ? new string[0] : request.amr_values.Split(' ')
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
                ClientAssertion = viewModel.ClientAssertion,
                ClientAssertionType = viewModel.ClientAssertionType,
                ClientId = viewModel.ClientId,
                ClientSecret = viewModel.ClientSecret,
                Token = viewModel.Token,
                TokenTypeHint = viewModel.TokenTypeHint
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

        public static AuthproviderResponse ToDto(this AuthenticationScheme authenticationScheme)
        {
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            return new AuthproviderResponse
            {
                AuthenticationScheme = authenticationScheme.Name,
                DisplayName = authenticationScheme.DisplayName
            };
        }

        public static IEnumerable<AuthproviderResponse> ToDtos(this IEnumerable<AuthenticationScheme> authenticationSchemes)
        {
            if (authenticationSchemes == null)
            {
                throw new ArgumentNullException(nameof(authenticationSchemes));
            }

            return authenticationSchemes.Select(a => a.ToDto());
        }

        public static ProfileResponse ToDto(this ResourceOwnerProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return new ProfileResponse
            {
                Issuer = profile.Issuer,
                UserId = profile.Subject,
                CreateDateTime = profile.CreateDateTime,
                UpdateTime = profile.UpdateTime
            };
        }

        public static SearchScopesParameter ToSearchScopesParameter(this SearchScopesRequest parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return new SearchScopesParameter
            {
                Count = parameter.NbResults,
                ScopeNames = parameter.ScopeNames,
                StartIndex = parameter.StartIndex,
                Types = parameter.ScopeTypes,
                Order = parameter.Order?.ToParameter()
            };
        }

        public static SearchClientParameter ToSearchClientParameter(this SearchClientsRequest parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            return new SearchClientParameter
            {
                ClientIds = parameter.ClientIds,
                ClientNames = parameter.ClientNames,
                ClientTypes = parameter.ClientTypes,
                Count = parameter.NbResults,
                StartIndex = parameter.StartIndex,
                Order = parameter.Order?.ToParameter()
            };
        }

        public static OrderParameter ToParameter(this OrderRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new OrderParameter
            {
                Target = request.Target,
                Type = (OrderTypes)request.Type
            };
        }

        public static SearchResourceOwnerParameter ToParameter(this SearchResourceOwnersRequest request)
        {
            return new SearchResourceOwnerParameter
            {
                Count = request.NbResults,
                StartIndex = request.StartIndex,
                Subjects = request.Subjects,
                Order = request.Order?.ToParameter()
            };
        }

        public static Scope ToParameter(this ScopeResponse scopeResponse)
        {
            if (scopeResponse == null)
            {
                throw new ArgumentNullException(nameof(scopeResponse));
            }

            return new Scope
            {
                Description = scopeResponse.Description,
                IsDisplayedInConsent = scopeResponse.IsDisplayedInConsent,
                IsExposed = scopeResponse.IsExposed,
                IsOpenIdScope = scopeResponse.IsOpenIdScope,
                Name = scopeResponse.Name,
                Type = (ScopeType)(int)scopeResponse.Type,
                Claims = scopeResponse.Claims
            };
        }

        public static SearchScopesResponse ToDto(this SearchScopeResult parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return new SearchScopesResponse
            {
                StartIndex = parameter.StartIndex,
                TotalResults = parameter.TotalResults,
                Content = parameter.Content == null ? new List<ScopeResponse>() : parameter.Content.Select(ToDto)
            };
        }

        public static PagedResponse<ResourceOwnerResponse> ToDto(this SearchResourceOwnerResult parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return new PagedResponse<ResourceOwnerResponse>
            {
                StartIndex = parameter.StartIndex,
                TotalResults = parameter.TotalResults,
                Content = parameter.Content == null ? new List<ResourceOwnerResponse>() : parameter.Content.Select(ToDto)
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

        public static ScopeResponse ToDto(this Scope scope)
        {
            return new ScopeResponse
            {
                Claims = scope.Claims.ToList(),
                Description = scope.Description,
                IsDisplayedInConsent = scope.IsDisplayedInConsent,
                IsExposed = scope.IsExposed,
                IsOpenIdScope = scope.IsOpenIdScope,
                Name = scope.Name,
                Type = (ScopeResponseType)(int)scope.Type,
                CreateDateTime = scope.CreateDateTime,
                UpdateDateTime = scope.UpdateDateTime
            };
        }

        public static List<ScopeResponse> ToDtos(this ICollection<Scope> scopes)
        {
            return scopes.Select(s => s.ToDto()).ToList();
        }

        public static List<ResourceOwnerResponse> ToDtos(this ICollection<ResourceOwner> resourceOwners)
        {
            return resourceOwners.Select(r => r.ToDto()).ToList();
        }

        public static AuthorizationParameter ToParameter(this AuthorizationRequest request)
        {
            var result = new AuthorizationParameter
            {
                AcrValues = request.AcrValues,
                ClientId = request.ClientId,
                Display = request.Display == null ? Display.page : (Display)request.Display,
                IdTokenHint = request.IdTokenHint,
                LoginHint = request.LoginHint,
                MaxAge = request.MaxAge,
                Nonce = request.Nonce,
                Prompt = request.Prompt,
                RedirectUrl = request.RedirectUri,
                ResponseMode = request.ResponseMode == null ? ResponseMode.None : (ResponseMode)request.ResponseMode,
                ResponseType = request.ResponseType,
                Scope = request.Scope,
                State = request.State,
                UiLocales = request.UiLocales,
                OriginUrl = request.OriginUrl,
                SessionId = request.SessionId,
                AmrValues = string.IsNullOrWhiteSpace(request.AmrValues) ? new string[0] : request.AmrValues.Split(' ')
            };

            if (!string.IsNullOrWhiteSpace(request.ProcessId))
            {
                result.ProcessId = request.ProcessId;
            }

            if (!string.IsNullOrWhiteSpace(request.Claims))
            {
                var claimsParameter = new ClaimsParameter();
                result.Claims = claimsParameter;

                var obj = JObject.Parse(request.Claims);
                var idToken = obj.GetValue(CoreConstants.StandardClaimParameterNames.IdTokenName);
                var userInfo = obj.GetValue(CoreConstants.StandardClaimParameterNames.UserInfoName);
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

            if (!string.IsNullOrWhiteSpace(request.CodeChallenge) && request.CodeChallengeMethod != null)
            {
                result.CodeChallenge = request.CodeChallenge;
                result.CodeChallengeMethod = (CodeChallengeMethods)request.CodeChallengeMethod;
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

            if (string.IsNullOrWhiteSpace(responseMode) || !Enum.TryParse(responseMode, out ResponseModes responseModeEnum))
            {
                responseModeEnum = ResponseModes.None;
            }

            var result = new AuthorizationRequest
            {
                AcrValues = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.AcrValuesName),
                Claims = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ClaimsName),
                ClientId = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ClientIdName),
                Display = displayEnum,
                Prompt = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.PromptName),
                IdTokenHint = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.IdTokenHintName),
                MaxAge = long.Parse(jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.MaxAgeName)),
                Nonce = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
                ResponseType = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                State = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.StateName),
                LoginHint = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.LoginHintName),
                RedirectUri = new Uri(jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.RedirectUriName)),
                Request = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.RequestName),
                RequestUri = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.RequestUriName),
                Scope = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ScopeName),
                ResponseMode = responseModeEnum,
                UiLocales = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.UiLocalesName),
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