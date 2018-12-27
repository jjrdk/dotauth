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

using DomainResponse = SimpleIdentityServer.Uma.Core.Responses;

namespace SimpleIdentityServer.Uma.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.DTOs;
    using Models;
    using Parameters;
    using SimpleAuth.Parameters;
    using SimpleAuth.Results;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;
    using ConfigurationResponse = Common.DTOs.ConfigurationResponse;

    internal static class MappingExtensions
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
            var claims = policyRule.Claims == null ? new List<PostClaim>()
                : policyRule.Claims.Select(p => p.ToResponse()).ToList();
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

        public static PostClaim ToResponse(this Claim claim)
        {
            return new PostClaim
            {
                Type = claim.Type,
                Value = claim.Value
            };
        }

        public static ConfigurationResponse ToResponse(this DomainResponse.ConfigurationResponse configuration)
        {
            return new ConfigurationResponse
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
                UserName = request.Username,
                Password = request.Password,
                Scope = request.Scope,
                ClientId = request.ClientId,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientSecret = request.ClientSecret
            };
        }

        public static AuthorizationCodeGrantTypeParameter ToAuthorizationCodeGrantTypeParameter(this TokenRequest request)
        {
            return new AuthorizationCodeGrantTypeParameter
            {
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Code = request.Code,
                RedirectUri = request.RedirectUri,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                CodeVerifier = request.CodeVerifier
            };
        }

        public static RefreshTokenGrantTypeParameter ToRefreshTokenGrantTypeParameter(this TokenRequest request)
        {
            return new RefreshTokenGrantTypeParameter
            {
                RefreshToken = request.RefreshToken
            };
        }

        public static ClientCredentialsGrantTypeParameter ToClientCredentialsGrantTypeParameter(this TokenRequest request)
        {
            return new ClientCredentialsGrantTypeParameter
            {
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Scope = request.Scope
            };
        }

        public static GetTokenViaTicketIdParameter ToTokenIdGrantTypeParameter(this TokenRequest request)
        {
            return new GetTokenViaTicketIdParameter
            {
                ClaimToken = request.ClaimToken,
                ClaimTokenFormat = request.ClaimTokenFormat,
                ClientId = request.ClientId,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientSecret = request.ClientSecret,
                Pct = request.Pct,
                Rpt = request.Rpt,
                Ticket = request.Ticket
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
                ClientAssertion = revocationRequest.ClientAssertion,
                ClientAssertionType = revocationRequest.ClientAssertionType,
                ClientId = revocationRequest.ClientId,
                ClientSecret = revocationRequest.ClientSecret,
                Token = revocationRequest.Token,
                TokenTypeHint = revocationRequest.TokenTypeHint
            };
        }
    }
}
