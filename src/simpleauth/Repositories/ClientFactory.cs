namespace SimpleAuth.Repositories
{
    using Errors;
    using Exceptions;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class ClientFactory
    {
        private readonly HttpClient _httpClient;
        private readonly IScopeStore _scopeRepository;

        public ClientFactory(HttpClient httpClient, IScopeStore scopeRepository)
        {
            _httpClient = httpClient;
            _scopeRepository = scopeRepository;
        }

        public async Task<Client> Build(Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            //ValidateNotMandatoryUri(newClient.LogoUri, SharedConstants.ClientNames.LogoUri);
            ValidateNotMandatoryUri(newClient.ClientUri, SharedConstants.ClientNames.ClientUri);
            ValidateNotMandatoryUri(newClient.TosUri, SharedConstants.ClientNames.TosUri);
            //ValidateNotMandatoryUri(newClient.JwksUri, SharedConstants.ClientNames.JwksUri);
            ValidateNotMandatoryUri(newClient.SectorIdentifierUri, SharedConstants.ClientNames.SectorIdentifierUri, true);

            // Based on the RFC : http://openid.net/specs/openid-connect-registration-1_0.html#SectorIdentifierValidation validate the sector_identifier_uri
            if (newClient.SectorIdentifierUri != null)
            {
                var sectorIdentifierUris =
                    await GetSectorIdentifierUris(newClient.SectorIdentifierUri).ConfigureAwait(false);
                if (sectorIdentifierUris.Any(sectorIdentifierUri =>
                    !newClient.RedirectionUrls.Contains(sectorIdentifierUri)))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidClientMetaData,
                        ErrorDescriptions.OneOrMoreSectorIdentifierUriIsNotARedirectUri);
                }
            }

            if (!string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseEnc))
            {
                if (string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseAlg))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidClientMetaData,
                        ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
                }
            }

            if (!string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseEnc))
            {
                if (string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseAlg))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidClientMetaData,
                        ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
                }
            }

            if (!string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionEnc))
            {
                if (string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionAlg))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidClientMetaData,
                        ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
                }
            }

            ValidateNotMandatoryUri(
                newClient.InitiateLoginUri,
               SharedConstants.ClientNames.InitiateLoginUri,
                true);

            if (newClient.RequestUris == null || !newClient.RequestUris.Any())
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.MissingParameter, SharedConstants.ClientNames.RequestUris));
            }

            if (newClient.RequestUris.Any(requestUri => !requestUri.IsAbsoluteUri))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    ErrorDescriptions.OneOfTheRequestUriIsNotValid);
            }

            var client = new Client
            {
                ClientId = string.IsNullOrWhiteSpace(newClient.ClientId)
                    ? Id.Create()
                    : newClient.ClientId
            };

            if (string.IsNullOrWhiteSpace(newClient.ClientName))
            {
                client.ClientName = "Unnamed_" + client.ClientId;
            }
            else
            {
                client.ClientName = newClient.ClientName;
            }

            client.TokenLifetime = newClient.TokenLifetime;
            client.ApplicationType = newClient.ApplicationType;
            client.ClientUri = newClient.ClientUri;
            client.Contacts = newClient.Contacts;
            client.DefaultAcrValues = newClient.DefaultAcrValues;
            //client.DefaultMaxAge = newClient.DefaultMaxAge;

            // If omitted then the default value is authorization code grant type
            if (newClient.GrantTypes == null ||
                !newClient.GrantTypes.Any())
            {
                client.GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code
                };
            }
            else
            {
                client.GrantTypes = newClient.GrantTypes;
            }

            client.IdTokenEncryptedResponseAlg = !string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseAlg)
                ? newClient.IdTokenEncryptedResponseAlg
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(client.IdTokenEncryptedResponseAlg))
            {
                client.IdTokenEncryptedResponseEnc = !string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseEnc)
                    ? newClient.IdTokenEncryptedResponseEnc
                    : SecurityAlgorithms.Aes128CbcHmacSha256;
            }
            else if(!string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseEnc))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClientMetaData,
                    ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
            }

            client.IdTokenSignedResponseAlg = !string.IsNullOrWhiteSpace(newClient.IdTokenSignedResponseAlg) ? newClient.IdTokenSignedResponseAlg : SecurityAlgorithms.RsaSha256;

            client.InitiateLoginUri = newClient.InitiateLoginUri;

            if (newClient.JsonWebKeys.Keys.Any() != true)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClientMetaData, ErrorDescriptions.JwkIsInvalid);
                //if (newClient.JwksUri != null)
                //{
                //    throw new SimpleAuthException(
                //        ErrorCodes.InvalidClientMetaData,
                //        ErrorDescriptions.TheJwksParameterCannotBeSetBecauseJwksUrlIsUsed);
                //}
            }

            client.JsonWebKeys = newClient.JsonWebKeys;
            //client.JwksUri = newClient.JwksUri;
            //client.LogoUri = newClient.LogoUri;
            client.PolicyUri = newClient.PolicyUri;
            client.PostLogoutRedirectUris = newClient.PostLogoutRedirectUris;

            if (newClient.AllowedScopes == null || newClient.AllowedScopes.Count == 0)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidScope,
                    string.Format(ErrorDescriptions.MissingParameter, SharedConstants.ClientNames.AllowedScopes));
            }

            var scopes = await _scopeRepository.SearchByNames(newClient.AllowedScopes.Select(s => s.Name))
                .ConfigureAwait(false);
            if (scopes.Count != newClient.AllowedScopes.Count)
            {
                var enumerable = newClient.AllowedScopes.Select(x => x.Name).Except(scopes.Select(x => x.Name));
                throw new SimpleAuthException(
                    ErrorCodes.InvalidScope,
                    $"Unknown scopes: {string.Join(",", enumerable)}");
            }

            client.AllowedScopes = newClient.AllowedScopes.ToArray();

            if (newClient.RedirectionUrls == null || newClient.RedirectionUrls.Count == 0)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRedirectUri,
                    string.Format(ErrorDescriptions.MissingParameter, SharedConstants.ClientNames.RedirectUris));
            }

            // Check the newClients when the application type is web
            if (client.ApplicationType == ApplicationTypes.web)
            {
                foreach (var redirectUri in newClient.RedirectionUrls)
                {
                    if (!Uri.IsWellFormedUriString(redirectUri.AbsoluteUri, UriKind.Absolute))
                    {
                        throw new SimpleAuthException(ErrorCodes.InvalidRedirectUri,
                            string.Format(ErrorDescriptions.TheRedirectUrlIsNotValid, redirectUri));
                    }

                    if (!string.IsNullOrWhiteSpace(redirectUri.Fragment))
                    {
                        throw new SimpleAuthException(ErrorCodes.InvalidRedirectUri,
                            string.Format(ErrorDescriptions.TheRedirectUrlCannotContainsFragment, redirectUri));
                    }

                    client.RedirectionUrls.Add(redirectUri);
                }
            }
            else
            {
                foreach (var redirectUri in newClient.RedirectionUrls)
                {
                    if (!Uri.IsWellFormedUriString(redirectUri.AbsoluteUri, UriKind.Absolute))
                    {
                        throw new SimpleAuthException(ErrorCodes.InvalidRedirectUri,
                            string.Format(ErrorDescriptions.TheRedirectUrlIsNotValid, redirectUri));
                    }

                    client.RedirectionUrls.Add(redirectUri);
                }
            }

            client.RequestObjectEncryptionAlg = newClient.RequestObjectEncryptionAlg;
            client.RequestObjectEncryptionEnc = newClient.RequestObjectEncryptionEnc;
            client.RequestObjectSigningAlg = newClient.RequestObjectSigningAlg;
            client.RequestUris = newClient.RequestUris;
            client.RequireAuthTime = newClient.RequireAuthTime;
            client.RequirePkce = newClient.RequirePkce;

            // If omitted then the default value is authorization code response type
            if (newClient.ResponseTypes?.Any() != true)
            {
                client.ResponseTypes = new[]
                {
                    ResponseTypeNames.Code
                };
            }
            else
            {
                client.ResponseTypes = newClient.ResponseTypes;
            }

            client.ScimProfile = newClient.ScimProfile;
            client.Secrets = newClient.Secrets;
            client.SectorIdentifierUri = newClient.SectorIdentifierUri;
            //client.SubjectType = newClient.SubjectType;

            if (client.Secrets.Count == 0 &&
                client.TokenEndPointAuthMethod != TokenEndPointAuthenticationMethods.private_key_jwt)
            {
                client.Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty)
                    }
                };
            }

            client.TokenEndPointAuthMethod = newClient.TokenEndPointAuthMethod;
            client.TokenEndPointAuthSigningAlg = newClient.TokenEndPointAuthSigningAlg;
            client.TosUri = newClient.TosUri;
            client.UserInfoEncryptedResponseAlg = newClient.UserInfoEncryptedResponseAlg;
            client.UserInfoEncryptedResponseEnc = newClient.UserInfoEncryptedResponseEnc;

            if (!string.IsNullOrWhiteSpace(newClient.UserInfoSignedResponseAlg))
            {
                client.UserInfoSignedResponseAlg = newClient.UserInfoSignedResponseAlg;
            }
            else
            {
                client.UserInfoSignedResponseAlg = SecurityAlgorithms.None;
            }

            if (!string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseAlg))
            {
                client.UserInfoEncryptedResponseAlg = newClient.UserInfoEncryptedResponseAlg;
            }
            else
            {
                client.UserInfoEncryptedResponseAlg = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(client.UserInfoEncryptedResponseAlg))
            {
                if (!string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseEnc))
                {
                    client.UserInfoEncryptedResponseEnc = newClient.UserInfoEncryptedResponseEnc;
                }
                else
                {
                    client.UserInfoEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256;
                }
            }

            if (!string.IsNullOrWhiteSpace(newClient.RequestObjectSigningAlg))
            {
                client.RequestObjectSigningAlg = newClient.RequestObjectSigningAlg;
            }
            else
            {
                client.RequestObjectSigningAlg = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionAlg))
            {
                client.RequestObjectEncryptionAlg = newClient.RequestObjectEncryptionAlg;
            }
            else
            {
                client.RequestObjectEncryptionAlg = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(client.RequestObjectEncryptionAlg))
            {
                if (!string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionEnc))
                {
                    client.RequestObjectEncryptionEnc = newClient.RequestObjectEncryptionEnc;
                }
                else
                {
                    client.RequestObjectEncryptionEnc = SecurityAlgorithms.Aes128CbcHmacSha256;
                }
            }

            if (!string.IsNullOrWhiteSpace(newClient.TokenEndPointAuthSigningAlg))
            {
                client.TokenEndPointAuthSigningAlg = newClient.TokenEndPointAuthSigningAlg;
            }
            else
            {
                client.TokenEndPointAuthSigningAlg = string.Empty;
            }

            return client;
        }

        private async Task<IReadOnlyCollection<Uri>> GetSectorIdentifierUris(Uri sectorIdentifierUri)
        {
            try
            {
                var response = _httpClient.GetAsync(sectorIdentifierUri).Result;
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return result.DeserializeWithJavascript<List<string>>().Select(x => new Uri(x)).ToList();
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    ErrorDescriptions.TheSectorIdentifierUrisCannotBeRetrieved,
                    ex);
            }
        }

        private static void ValidateNotMandatoryUri(
            Uri uri,
            string newClientName,
            bool checkSchemeIsHttps = false)
        {
            if (uri == null)
            {
                return;
            }

            if (!uri.IsAbsoluteUri || !Uri.IsWellFormedUriString(uri.AbsoluteUri, UriKind.Absolute))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    string.Format(ErrorDescriptions.ParameterIsNotCorrect, newClientName));
            }

            if (checkSchemeIsHttps && uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    string.Format(ErrorDescriptions.ParameterIsNotCorrect, newClientName));
            }
        }
    }
}
