namespace SimpleAuth.Shared.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;

    internal class ClientFactory
    {
        private readonly HttpClient _httpClient;
        private readonly IScopeStore _scopeRepository;
        private readonly Func<string, Uri[]> _urlReader;

        public ClientFactory(HttpClient httpClient, IScopeStore scopeRepository, Func<string, Uri[]> urlReader)
        {
            _httpClient = httpClient;
            _scopeRepository = scopeRepository;
            _urlReader = urlReader;
        }

        public async Task<Client> Build(Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            //ValidateNotMandatoryUri(newClient.LogoUri, "logo_uri");
            ValidateNotMandatoryUri(newClient.ClientUri, "client_uri");
            ValidateNotMandatoryUri(newClient.TosUri, "tos_uri");
            //ValidateNotMandatoryUri(newClient.JwksUri, "jwks_uri");
            ValidateNotMandatoryUri(
                newClient.SectorIdentifierUri,
                "sector_identifier_uri",
                true);

            // Based on the RFC : http://openid.net/specs/openid-connect-registration-1_0.html#SectorIdentifierValidation validate the sector_identifier_uri
            if (newClient.SectorIdentifierUri != null)
            {
                var sectorIdentifierUris =
                    await GetSectorIdentifierUris(newClient.SectorIdentifierUri).ConfigureAwait(false);
                if (sectorIdentifierUris.Any(
                    sectorIdentifierUri => !newClient.RedirectionUrls.Contains(sectorIdentifierUri)))
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

            ValidateNotMandatoryUri(newClient.InitiateLoginUri, "initiate_login_uri", true);

            if (newClient.RequestUris == null || !newClient.RequestUris.Any())
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.MissingParameter, "request_uris"));
            }

            if (newClient.RequestUris.Any(requestUri => !requestUri.IsAbsoluteUri))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    ErrorDescriptions.OneOfTheRequestUriIsNotValid);
            }

            var client = new Client
            {
                ClientId = string.IsNullOrWhiteSpace(newClient.ClientId) ? Id.Create() : newClient.ClientId
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
            if (newClient.GrantTypes == null || !newClient.GrantTypes.Any())
            {
                client.GrantTypes = new[] {GrantTypes.AuthorizationCode};
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
            else if (!string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseEnc))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
            }

            client.IdTokenSignedResponseAlg = !string.IsNullOrWhiteSpace(newClient.IdTokenSignedResponseAlg)
                ? newClient.IdTokenSignedResponseAlg
                : SecurityAlgorithms.RsaSha256;

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

            if (newClient.AllowedScopes == null || newClient.AllowedScopes.Length == 0)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidScope,
                    string.Format(ErrorDescriptions.MissingParameter, "allowed_scopes"));
            }

            var scopes = await _scopeRepository.SearchByNames(
                    CancellationToken.None,
                    newClient.AllowedScopes)
                .ConfigureAwait(false);
            if (scopes.Length != newClient.AllowedScopes.Length)
            {
                var enumerable = newClient.AllowedScopes.Except(scopes.Select(x => x.Name));
                throw new SimpleAuthException(
                    ErrorCodes.InvalidScope,
                    $"Unknown scopes: {string.Join(",", enumerable)}");
            }

            client.AllowedScopes = newClient.AllowedScopes.ToArray();

            if (newClient.RedirectionUrls == null || newClient.RedirectionUrls.Count == 0)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRedirectUri,
                    string.Format(ErrorDescriptions.MissingParameter, "redirect_uris"));
            }

            // Check the newClients when the application type is web
            if (client.ApplicationType == ApplicationTypes.Web)
            {
                foreach (var redirectUri in newClient.RedirectionUrls)
                {
                    if (!Uri.IsWellFormedUriString(redirectUri.AbsoluteUri, UriKind.Absolute))
                    {
                        throw new SimpleAuthException(
                            ErrorCodes.InvalidRedirectUri,
                            string.Format(ErrorDescriptions.TheRedirectUrlIsNotValid, redirectUri));
                    }

                    if (!string.IsNullOrWhiteSpace(redirectUri.Fragment))
                    {
                        throw new SimpleAuthException(
                            ErrorCodes.InvalidRedirectUri,
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
                        throw new SimpleAuthException(
                            ErrorCodes.InvalidRedirectUri,
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
                client.ResponseTypes = new[] {ResponseTypeNames.Code};
            }
            else
            {
                client.ResponseTypes = newClient.ResponseTypes;
            }

            client.ScimProfile = newClient.ScimProfile;
            client.Secrets = newClient.Secrets;
            client.SectorIdentifierUri = newClient.SectorIdentifierUri;
            //client.SubjectType = newClient.SubjectType;

            if (client.Secrets.Length == 0
                && client.TokenEndPointAuthMethod != TokenEndPointAuthenticationMethods.PrivateKeyJwt)
            {
                client.Secrets = new []
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

            client.UserInfoSignedResponseAlg = !string.IsNullOrWhiteSpace(newClient.UserInfoSignedResponseAlg)
                ? newClient.UserInfoSignedResponseAlg
                : SecurityAlgorithms.None;

            client.UserInfoEncryptedResponseAlg = !string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseAlg)
                ? newClient.UserInfoEncryptedResponseAlg
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(client.UserInfoEncryptedResponseAlg))
            {
                client.UserInfoEncryptedResponseEnc = !string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseEnc)
                    ? newClient.UserInfoEncryptedResponseEnc
                    : SecurityAlgorithms.Aes128CbcHmacSha256;
            }

            client.RequestObjectSigningAlg = !string.IsNullOrWhiteSpace(newClient.RequestObjectSigningAlg)
                ? newClient.RequestObjectSigningAlg
                : string.Empty;

            client.RequestObjectEncryptionAlg = !string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionAlg)
                ? newClient.RequestObjectEncryptionAlg
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(client.RequestObjectEncryptionAlg))
            {
                client.RequestObjectEncryptionEnc = !string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionEnc)
                    ? newClient.RequestObjectEncryptionEnc
                    : SecurityAlgorithms.Aes128CbcHmacSha256;
            }

            client.TokenEndPointAuthSigningAlg = !string.IsNullOrWhiteSpace(newClient.TokenEndPointAuthSigningAlg)
                ? newClient.TokenEndPointAuthSigningAlg
                : string.Empty;

            return client;
        }

        private async Task<IReadOnlyCollection<Uri>> GetSectorIdentifierUris(Uri sectorIdentifierUri)
        {
            try
            {
                var response = _httpClient.GetAsync(sectorIdentifierUri).Result;
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return _urlReader(result);
                //result.DeserializeWithJavascript<List<string>>().Select(x => new Uri(x)).ToList();
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
