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

namespace DotAuth.Extensions;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using DotAuth.Common;
using DotAuth.Parameters;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;

internal static class MappingExtensions
{
    public static GrantedTokenResponse ToDto(this GrantedToken grantedToken)
    {
        return new()
        {
            AccessToken = grantedToken.AccessToken,
            IdToken = grantedToken.IdToken,
            ExpiresIn = grantedToken.ExpiresIn,
            RefreshToken = grantedToken.RefreshToken,
            TokenType = grantedToken.TokenType,
            Scope = string.Join(' ', grantedToken.Scope)
        };
    }

    public static ResourceOwnerGrantTypeParameter ToResourceOwnerGrantTypeParameter(this TokenRequest request)
    {
        return new()
        {
            UserName = request.username,
            Password = request.password,
            Scope = request.scope,
            ClientId = request.client_id,
            ClientAssertion = request.client_assertion,
            ClientAssertionType = request.client_assertion_type,
            ClientSecret = request.client_secret,
            AmrValues = string.IsNullOrWhiteSpace(request.amr_values)
                ? Array.Empty<string>()
                : request.amr_values.Split(' ')
        };
    }

    public static AuthorizationCodeGrantTypeParameter ToAuthorizationCodeGrantTypeParameter(this TokenRequest request)
    {
        return new()
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
        return new()
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
        return new()
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
        return new()
        {
            ClaimToken = new ClaimTokenParameter
            {
                Token = request.claim_token,
                Format = request.claim_token_format
            },
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
        return new()
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
        return new()
        {
            ClientAssertion = revocationRequest.client_assertion,
            ClientAssertionType = revocationRequest.client_assertion_type,
            ClientId = revocationRequest.client_id,
            ClientSecret = revocationRequest.client_secret,
            Token = revocationRequest.token,
            TokenTypeHint = revocationRequest.token_type_hint
        };
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
            AmrValues = string.IsNullOrWhiteSpace(request.amr_values)
                ? Array.Empty<string>()
                : request.amr_values.Split(' ')
        };

        if (!string.IsNullOrWhiteSpace(request.aggregate_id))
        {
            result = result with { ProcessId = request.aggregate_id };
        }

        if (!string.IsNullOrWhiteSpace(request.claims))
        {
            result = result with { Claims = new ClaimsParameter() };

            var obj = JsonNode.Parse(request.claims);
            if (obj is JsonObject jsonObject)
            {
                var hasIdToken = jsonObject.TryGetPropertyValue(CoreConstants.StandardClaimParameterNames.IdTokenName, out var idToken);
                var hasUserInfo = jsonObject.TryGetPropertyValue(CoreConstants.StandardClaimParameterNames.UserInfoName, out var userInfo);
                if (hasIdToken)
                {
                    result = result with
                    {
                        Claims = result.Claims with
                        {
                            IdToken = FillInClaimsParameter(idToken!, result.Claims.IdToken)
                        }
                    };
                }

                if (hasUserInfo)
                {
                    result = result with
                    {
                        Claims = result.Claims with
                        {
                            UserInfo = FillInClaimsParameter(userInfo!, result.Claims.UserInfo)
                        }
                    };
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(request.code_challenge) && request.code_challenge_method != null)
        {
            result = result with
            {
                CodeChallenge = request.code_challenge,
                CodeChallengeMethod = request.code_challenge_method
            };
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
            acr_values =
                jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.AcrValuesName),
            claims = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ClaimsName),
            client_id = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ClientIdName),
            display = displayEnum,
            prompt = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.PromptName),
            id_token_hint =
                jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.IdTokenHintName),
            max_age = long.Parse(
                jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.MaxAgeName)!),
            nonce = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
            response_type =
                jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
            state = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.StateName),
            login_hint =
                jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.LoginHintName),
            redirect_uri =
                new Uri(jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames
                    .RedirectUriName)!),
            request = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.RequestName),
            request_uri =
                new Uri(jwsPayload.GetClaimValue(
                    CoreConstants.StandardAuthorizationRequestParameterNames.RequestUriName)!),
            scope = jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.ScopeName),
            response_mode = string.IsNullOrWhiteSpace(responseMode) ? ResponseModes.None : responseMode,
            ui_locales =
                jwsPayload.GetClaimValue(CoreConstants.StandardAuthorizationRequestParameterNames.UiLocalesName),
        };

        return result;
    }

    public static Permission ToPermission(this TicketLine ticketLine, TimeSpan rptLifetime = default)
    {
        var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return new Permission
        {
            Scopes = ticketLine.Scopes,
            ResourceSetId = ticketLine.ResourceSetId,
            Expiry = DateTimeOffset.UtcNow.Add(rptLifetime).ToUnixTimeSeconds(),
            IssuedAt = iat,
            NotBefore = iat
        };
    }

    private static ClaimParameter[] FillInClaimsParameter(
        JsonNode token,
        IEnumerable<ClaimParameter> claimParameters)
    {
        var children = token.AsObject()
            .Select(
                child =>
                {
                    var record = new ClaimParameter
                    {
                        Name = child.Key,
                        Parameters = new Dictionary<string, object>()
                    };

                    var subChild = child.Value?.AsObject().FirstOrDefault();
                    if (subChild?.Value != null)
                    {
                        JsonNode? node = subChild.Value.Value!;
                        var parameters =
                            JsonSerializer.Deserialize<Dictionary<string, object>>(node.ToJsonString(),
                                DefaultJsonSerializerOptions.Instance)!;
                        record = record with { Parameters = parameters };
                    }

                    return record;
                });
        return claimParameters.Concat(children).ToArray();
    }
}
