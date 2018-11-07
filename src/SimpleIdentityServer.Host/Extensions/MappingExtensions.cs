// Copyright 2015 Habart Thierry
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Host.Extensions
{
    using Manager.Common.Requests;
    using Manager.Common.Responses;
    using Manager.Core.Parameters;
    using Manager.Core.Results;
    using Shared;
    using Shared.Models;
    using Shared.Parameters;
    using Shared.Requests;
    using Shared.Responses;
    using Shared.Results;
    using CodeChallengeMethods = Shared.Models.CodeChallengeMethods;

    public static class MappingExtensions
    {
        public static AddClaimParameter ToParameter(this ClaimResponse request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new AddClaimParameter
            {
                Code = request.Code,
                IsIdentifier = request.IsIdentifier
            };
        }

        public static SearchClaimsParameter ToParameter(this SearchClaimsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new SearchClaimsParameter
            {
                Count = request.NbResults,
                ClaimKeys = request.Codes,
                StartIndex = request.StartIndex,
                Order = request.Order?.ToParameter()
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

        public static UpdateResourceOwnerClaimsParameter ToParameter(this UpdateResourceOwnerClaimsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new UpdateResourceOwnerClaimsParameter
            {
                Login = request.Login,
                Claims = request.Claims
            };
        }

        public static UpdateResourceOwnerPasswordParameter ToParameter(this UpdateResourceOwnerPasswordRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new UpdateResourceOwnerPasswordParameter
            {
                Login = request.Login,
                Password = request.Password
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

        public static AddUserParameter ToParameter(this AddResourceOwnerRequest request)
        {
            return new AddUserParameter(request.Subject, request.Password);
        }

        public static GetJwsParameter ToParameter(this GetJwsRequest getJwsRequest)
        {
            return new GetJwsParameter
            {
                Jws = getJwsRequest.Jws,
                Url = getJwsRequest.Url
            };
        }

        public static GetJweParameter ToParameter(this GetJweRequest getJweRequest)
        {
            return new GetJweParameter
            {
                Jwe = getJweRequest.Jwe,
                Password = getJweRequest.Password,
                Url = getJweRequest.Url
            };
        }

        public static CreateJweParameter ToParameter(this CreateJweRequest createJweRequest)
        {
            return new CreateJweParameter
            {
                Alg = createJweRequest.Alg,
                Enc = createJweRequest.Enc,
                Jws = createJweRequest.Jws,
                Kid = createJweRequest.Kid,
                Password = createJweRequest.Password,
                Url = createJweRequest.Url
            };
        }

        public static CreateJwsParameter ToParameter(this CreateJwsRequest createJwsRequest)
        {
            return new CreateJwsParameter
            {
                Alg = createJwsRequest.Alg,
                Kid = createJwsRequest.Kid,
                Url = createJwsRequest.Url,
                Payload = createJwsRequest.Payload
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

        public static UpdateClientParameter ToParameter(this UpdateClientRequest updateClientRequest)
        {
            return new UpdateClientParameter
            {
                ApplicationType = updateClientRequest.ApplicationType,
                ClientId = updateClientRequest.ClientId,
                ClientName = updateClientRequest.ClientName,
                ClientUri = updateClientRequest.ClientUri,
                Contacts = updateClientRequest.Contacts,
                DefaultAcrValues = updateClientRequest.DefaultAcrValues,
                DefaultMaxAge = updateClientRequest.DefaultMaxAge,
                GrantTypes = updateClientRequest.GrantTypes,
                IdTokenEncryptedResponseAlg = updateClientRequest.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = updateClientRequest.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = updateClientRequest.IdTokenSignedResponseAlg,
                InitiateLoginUri = updateClientRequest.InitiateLoginUri,
                Jwks = updateClientRequest.Jwks,
                JwksUri = updateClientRequest.JwksUri,
                LogoUri = updateClientRequest.LogoUri,
                PolicyUri = updateClientRequest.PolicyUri,
                RedirectUris = updateClientRequest.RedirectUris,
                RequestObjectEncryptionAlg = updateClientRequest.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = updateClientRequest.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = updateClientRequest.RequestObjectSigningAlg,
                RequestUris = updateClientRequest.RequestUris,
                RequireAuthTime = updateClientRequest.RequireAuthTime,
                ResponseTypes = updateClientRequest.ResponseTypes,
                SectorIdentifierUri = updateClientRequest.SectorIdentifierUri,
                SubjectType = updateClientRequest.SubjectType,
                TokenEndPointAuthMethod = updateClientRequest.TokenEndPointAuthMethod,
                TokenEndPointAuthSigningAlg = updateClientRequest.TokenEndPointAuthSigningAlg,
                TosUri = updateClientRequest.TosUri,
                UserInfoEncryptedResponseAlg = updateClientRequest.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = updateClientRequest.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = updateClientRequest.UserInfoSignedResponseAlg,
                PostLogoutRedirectUris = updateClientRequest.PostLogoutRedirectUris,
                AllowedScopes = updateClientRequest.AllowedScopes ?? new List<string>()
            };
        }

        public static RegistrationParameter ToParameter(this AddClientRequest clientResponse)
        {
            var redirectUris = clientResponse.RedirectUris == null
                ? new List<string>()
                : clientResponse.RedirectUris.ToList();
            return new RegistrationParameter
            {
                ApplicationType = clientResponse.ApplicationType,
                ClientName = clientResponse.ClientName,
                ClientUri = clientResponse.ClientUri,
                Contacts = clientResponse.Contacts == null ? new List<string>() : clientResponse.Contacts.ToList(),
                DefaultAcrValues = clientResponse.DefaultAcrValues,
                DefaultMaxAge = clientResponse.DefaultMaxAge,
                GrantTypes = clientResponse.GrantTypes,
                IdTokenEncryptedResponseAlg = clientResponse.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = clientResponse.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = clientResponse.IdTokenSignedResponseAlg,
                InitiateLoginUri = clientResponse.InitiateLoginUri,
                Jwks = clientResponse.Jwks,
                JwksUri = clientResponse.JwksUri,
                LogoUri = clientResponse.LogoUri,
                PolicyUri = clientResponse.PolicyUri,
                RedirectUris = redirectUris,
                RequestObjectEncryptionAlg = clientResponse.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = clientResponse.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = clientResponse.RequestObjectSigningAlg,
                RequestUris = clientResponse.RequestUris,
                RequireAuthTime = clientResponse.RequireAuthTime,
                ResponseTypes = clientResponse.ResponseTypes,
                SectorIdentifierUri = clientResponse.SectorIdentifierUri,
                SubjectType = clientResponse.SubjectType,
                TokenEndPointAuthMethod = clientResponse.TokenEndPointAuthMethod,
                TokenEndPointAuthSigningAlg = clientResponse.TokenEndPointAuthSigningAlg,
                PostLogoutRedirectUris = clientResponse.PostLogoutRedirectUris,
                TosUri = clientResponse.TosUri,
                UserInfoEncryptedResponseAlg = clientResponse.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = clientResponse.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = clientResponse.UserInfoSignedResponseAlg
            };
        }

        public static Client ToModel(this ClientResponse clientResponse)
        {
            var responseTypes = new List<ResponseType>();
            var grantTypes = new List<GrantType>();
            var secrets = new List<ClientSecret>();
            var redirectUris = clientResponse.RedirectUris == null
                ? new List<string>()
                : clientResponse.RedirectUris.ToList();
            var scopes = clientResponse.AllowedScopes == null ? new List<Scope>() : clientResponse.AllowedScopes.Select(s => new Scope
            {
                Name = s
            }).ToList();
            ApplicationTypes? applicationType = null;
            if (clientResponse.ResponseTypes != null &&
                clientResponse.ResponseTypes.Any())
            {
                foreach (var responseType in clientResponse.ResponseTypes)
                {
                    var responseTypeSplitted = responseType.Split(' ');
                    foreach (var response in responseTypeSplitted)
                    {
                        if (Enum.TryParse(response, out ResponseType responseTypeEnum) &&
                            !responseTypes.Contains(responseTypeEnum))
                        {
                            responseTypes.Add(responseTypeEnum);
                        }
                    }
                }
            }

            if (clientResponse.GrantTypes != null &&
                clientResponse.GrantTypes.Any())
            {
                foreach (var grantType in clientResponse.GrantTypes)
                {
                    if (Enum.TryParse(grantType, out GrantType grantTypeEnum))
                    {
                        grantTypes.Add(grantTypeEnum);
                    }
                }
            }

            if (clientResponse.Secrets != null && clientResponse.Secrets.Any())
            {
                secrets.AddRange(clientResponse.Secrets.Select(secret => new ClientSecret { Type = secret.Type, Value = secret.Value }));
            }

            if (Enum.TryParse(clientResponse.ApplicationType, out ApplicationTypes appTypeEnum))
            {
                applicationType = appTypeEnum;
            }

            if (!Enum.TryParse(clientResponse.TokenEndPointAuthMethod, out TokenEndPointAuthenticationMethods tokenEndPointAuthenticationMethod))
            {
                tokenEndPointAuthenticationMethod = TokenEndPointAuthenticationMethods.client_secret_basic;
            }

            return new Client
            {
                AllowedScopes = scopes,
                GrantTypes = grantTypes,
                TokenEndPointAuthMethod = tokenEndPointAuthenticationMethod,
                ApplicationType = appTypeEnum,
                ResponseTypes = responseTypes,
                ClientId = clientResponse.ClientId,
                ClientName = clientResponse.ClientName,
                Secrets = secrets,
                // ClientSecret = clientResponse.ClientSecret,
                ClientUri = clientResponse.ClientUri,
                Contacts = clientResponse.Contacts,
                DefaultAcrValues = clientResponse.DefaultAcrValues,
                DefaultMaxAge = clientResponse.DefaultMaxAge,
                IdTokenEncryptedResponseAlg = clientResponse.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = clientResponse.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = clientResponse.IdTokenSignedResponseAlg,
                InitiateLoginUri = clientResponse.InitiateLoginUri,
                JwksUri = clientResponse.JwksUri,
                LogoUri = clientResponse.LogoUri,
                PolicyUri = clientResponse.PolicyUri,
                UserInfoSignedResponseAlg = clientResponse.UserInfoSignedResponseAlg,
                UserInfoEncryptedResponseEnc = clientResponse.UserInfoEncryptedResponseEnc,
                UserInfoEncryptedResponseAlg = clientResponse.UserInfoEncryptedResponseAlg,
                TosUri = clientResponse.TosUri,
                TokenEndPointAuthSigningAlg = clientResponse.TokenEndPointAuthSigningAlg,
                SubjectType = clientResponse.SubjectType,
                SectorIdentifierUri = clientResponse.SectorIdentifierUri,
                RequireAuthTime = clientResponse.RequireAuthTime,
                RequestObjectSigningAlg = clientResponse.RequestObjectSigningAlg,
                RequestObjectEncryptionAlg = clientResponse.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = clientResponse.RequestObjectEncryptionEnc,
                RedirectionUrls = redirectUris,
                RequestUris = clientResponse.RequestUris
            };
        }

        public static ImportParameter ToParameter(this ExportResponse export)
        {
            if (export == null)
            {
                throw new ArgumentNullException(nameof(export));
            }


            return new ImportParameter
            {
                Clients = export.Clients?.Select(c => c.ToModel())
            };
        }

        public static ClaimResponse ToDto(this ClaimAggregate claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            return new ClaimResponse
            {
                Code = claim.Code,
                CreateDateTime = claim.CreateDateTime,
                IsIdentifier = claim.IsIdentifier,
                UpdateDateTime = claim.UpdateDateTime
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
                Content = parameter.Content == null ? new List<ScopeResponse>() : parameter.Content.Select(c => ToDto(c))
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
                Content = parameter.Content == null ? new List<ResourceOwnerResponse>() : parameter.Content.Select(c => ToDto(c))
            };
        }

        public static PagedResponse<ClientResponse> ToDto(this SearchClientResult parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return new PagedResponse<ClientResponse>
            {
                StartIndex = parameter.StartIndex,
                TotalResults = parameter.TotalResults,
                Content = parameter.Content == null ? new List<ClientResponse>() : parameter.Content.Select(c => ToDto(c))
            };
        }

        public static SearchClaimsResponse ToDto(this SearchClaimsResult parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return new SearchClaimsResponse
            {
                StartIndex = parameter.StartIndex,
                TotalResults = parameter.TotalResults,
                Content = parameter.Content == null ? new List<ClaimResponse>() : parameter.Content.Select(c => ToDto(c))
            };
        }

        public static JwsInformationResponse ToDto(this JwsInformationResult jwsInformationResult)
        {
            return new JwsInformationResponse
            {
                Header = jwsInformationResult.Header,
                JsonWebKey = jwsInformationResult.JsonWebKey,
                Payload = jwsInformationResult.Payload
            };
        }

        public static ExportResponse ToDto(this ExportResult export)
        {
            if (export == null)
            {
                throw new ArgumentNullException(nameof(export));
            }

            return new ExportResponse
            {
                Clients = export.Clients?.Select(c => c.ToDto())
            };
        }

        public static JweInformationResponse ToDto(this JweInformationResult jweInformationResult)
        {
            return new JweInformationResponse
            {
                IsContentJws = jweInformationResult.IsContentJws,
                Content = jweInformationResult.Content
            };
        }

        public static ResponseClientSecret ToDto(this ClientSecret secret)
        {
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret));
            }

            return new ResponseClientSecret
            {
                Type = secret.Type,
                Value = secret.Value
            };
        }

        public static ClientResponse ToDto(this Client client)
        {
            IEnumerable<ResponseClientSecret> secrets = null;
            if (client.Secrets != null)
            {
                secrets = client.Secrets.Select(s => s.ToDto());
            }

            return new ClientResponse
            {
                AllowedScopes = client.AllowedScopes == null ? new List<string>() : client.AllowedScopes.Select(c => c.Name).ToList(),
                ApplicationType = Enum.GetName(typeof(ApplicationTypes), client.ApplicationType),
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                Secrets = secrets,
                ClientUri = client.ClientUri,
                Contacts = client.Contacts,
                DefaultAcrValues = client.DefaultAcrValues,
                DefaultMaxAge = client.DefaultMaxAge,
                GrantTypes = client.GrantTypes == null ? new List<string>() : client.GrantTypes.Select(g => Enum.GetName(typeof(GrantType), g)).ToList(),
                IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
                InitiateLoginUri = client.InitiateLoginUri,
                JsonWebKeys = client.JsonWebKeys,
                JwksUri = client.JwksUri,
                LogoUri = client.LogoUri,
                PolicyUri = client.PolicyUri,
                RedirectUris = client.RedirectionUrls,
                RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = client.RequestObjectSigningAlg,
                RequestUris = client.RequestUris,
                RequireAuthTime = client.RequireAuthTime,
                ResponseTypes = client.ResponseTypes == null ? new List<string>() : client.ResponseTypes.Select(g => Enum.GetName(typeof(ResponseType), g)).ToList(),
                SectorIdentifierUri = client.SectorIdentifierUri,
                SubjectType = client.SubjectType,
                TokenEndPointAuthMethod = Enum.GetName(typeof(TokenEndPointAuthenticationMethods), client.TokenEndPointAuthMethod),
                TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg,
                UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg,
                TosUri = client.TosUri,
                PostLogoutRedirectUris = client.PostLogoutRedirectUris,
                CreateDateTime = client.CreateDateTime,
                UpdateDateTime = client.UpdateDateTime
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
                Claims = scope.Claims,
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

        public static List<ClientResponse> ToDtos(this IEnumerable<Client> clients)
        {
            return clients.Select(c => c.ToDto()).ToList();
        }

        public static List<ScopeResponse> ToDtos(this ICollection<Scope> scopes)
        {
            return scopes.Select(s => s.ToDto()).ToList();
        }

        public static List<ResourceOwnerResponse> ToDtos(this ICollection<ResourceOwner> resourceOwners)
        {
            return resourceOwners.Select(r => r.ToDto()).ToList();
        }

        public static IEnumerable<ClaimResponse> ToDtos(this IEnumerable<ClaimAggregate> claims)
        {
            return claims.Select(c => c.ToDto());
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
                var idToken = obj.GetValue(Core.Constants.StandardClaimParameterNames.IdTokenName);
                var userInfo = obj.GetValue(Core.Constants.StandardClaimParameterNames.UserInfoName);
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
                ClientSecret = request.ClientSecret,
                AmrValues = string.IsNullOrWhiteSpace(request.AmrValues) ? new string[0] : request.AmrValues.Split(' ')
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
                RefreshToken = request.RefreshToken,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret
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

        public static AuthorizationRequest ToAuthorizationRequest(this JwsPayload jwsPayload)
        {
            var displayVal =
                jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.DisplayName);
            var responseMode =
                jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.ResponseModeName);
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
                AcrValues = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.AcrValuesName),
                Claims = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.ClaimsName),
                ClientId = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.ClientIdName),
                Display = displayEnum,
                Prompt = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.PromptName),
                IdTokenHint = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.IdTokenHintName),
                MaxAge = jwsPayload.GetDoubleClaim(Core.Constants.StandardAuthorizationRequestParameterNames.MaxAgeName),
                Nonce = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.NonceName),
                ResponseType = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                State = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.StateName),
                LoginHint = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.LoginHintName),
                RedirectUri = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.RedirectUriName),
                Request = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.RequestName),
                RequestUri = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.RequestUriName),
                Scope = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.ScopeName),
                ResponseMode = responseModeEnum,
                UiLocales = jwsPayload.GetStringClaim(Core.Constants.StandardAuthorizationRequestParameterNames.UiLocalesName),
            };

            return result;
        }

        public static JwsPayload ToJwsPayload(this AuthorizationRequest request)
        {
            return new JwsPayload
            {
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.AcrValuesName, request.AcrValues
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ClaimsName, request.Claims
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ClientIdName, request.ClientId
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.DisplayName, Enum.GetName(typeof(Display), request.Display)
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.PromptName, request.Prompt
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.IdTokenHintName, request.IdTokenHint
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.MaxAgeName, request.MaxAge
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.NonceName, request.Nonce
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName, request.ResponseType
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.StateName, request.State
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.LoginHintName, request.LoginHint
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.RedirectUriName, request.RedirectUri
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.RequestName, request.Request
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.RequestUriName, request.RequestUri
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ScopeName, request.Scope
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ResponseModeName, Enum.GetName(typeof(ResponseModes), request.ResponseMode)
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.UiLocalesName, request.UiLocales
                }
            };
        }

        public static RegistrationParameter ToParameter(this ClientRequest clientRequest)
        {
            if (clientRequest == null)
            {
                throw new ArgumentNullException(nameof(clientRequest));
            }

            var responseTypes = new List<ResponseType>();
            var grantTypes = new List<GrantType>();
            ApplicationTypes? applicationType = null;
            if (clientRequest.ResponseTypes != null && clientRequest.ResponseTypes.Any())
            {
                foreach (var responseType in clientRequest.ResponseTypes)
                {
                    if (Enum.TryParse(responseType, out ResponseType responseTypeEnum) &&
                        !responseTypes.Contains(responseTypeEnum))
                    {
                        responseTypes.Add(responseTypeEnum);
                    }
                }
            }

            if (clientRequest.GrantTypes != null && clientRequest.GrantTypes.Any())
            {
                foreach (var grantType in clientRequest.GrantTypes)
                {
                    if (Enum.TryParse(grantType, out GrantType grantTypeEnum))
                    {
                        grantTypes.Add(grantTypeEnum);
                    }
                }
            }

            if (Enum.TryParse(clientRequest.ApplicationType, out ApplicationTypes appTypeEnum))
            {
                applicationType = appTypeEnum;
            }

            return new RegistrationParameter
            {
                ApplicationType = applicationType,
                ClientName = clientRequest.ClientName,
                ClientUri = clientRequest.ClientUri,
                Contacts = clientRequest.Contacts == null ? new List<string>() : clientRequest.Contacts.ToList(),
                DefaultAcrValues = clientRequest.DefaultAcrValues,
                DefaultMaxAge = clientRequest.DefaultMaxAge,
                GrantTypes = grantTypes,
                IdTokenEncryptedResponseAlg = clientRequest.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = clientRequest.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = clientRequest.IdTokenSignedResponseAlg,
                InitiateLoginUri = clientRequest.InitiateLoginUri,
                Jwks = clientRequest.Jwks,
                JwksUri = clientRequest.JwksUri,
                LogoUri = clientRequest.LogoUri,
                PolicyUri = clientRequest.PolicyUri,
                RedirectUris = clientRequest.RedirectUris == null ? new List<string>() : clientRequest.RedirectUris.ToList(),
                RequestObjectEncryptionAlg = clientRequest.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = clientRequest.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = clientRequest.RequestObjectSigningAlg,
                RequestUris = clientRequest.RequestUris == null ? new List<string>() : clientRequest.RequestUris.ToList(),
                RequireAuthTime = clientRequest.RequireAuthTime,
                ResponseTypes = responseTypes,
                SectorIdentifierUri = clientRequest.SectorIdentifierUri,
                SubjectType = clientRequest.SubjectType,
                TokenEndPointAuthMethod = clientRequest.TokenEndpointAuthMethod,
                TokenEndPointAuthSigningAlg = clientRequest.TokenEndpointAuthSigningAlg,
                TosUri = clientRequest.TosUri,
                UserInfoEncryptedResponseAlg = clientRequest.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = clientRequest.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = clientRequest.UserInfoSignedResponseAlg,
                ScimProfile = clientRequest.ScimProfile
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

        private static void FillInClaimsParameter(
            JToken token,
            List<ClaimParameter> claimParameters)
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