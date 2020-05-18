namespace SimpleAuth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

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

        public async Task<Client> Build(Client newClient, CancellationToken cancellationToken = default)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            ValidateNotMandatoryUri(newClient.ClientUri, "client_uri");
            ValidateNotMandatoryUri(newClient.TosUri, "tos_uri");
            ValidateNotMandatoryUri(newClient.SectorIdentifierUri, "sector_identifier_uri", true);

            // Based on the RFC : http://openid.net/specs/openid-connect-registration-1_0.html#SectorIdentifierValidation validate the sector_identifier_uri
            if (newClient.SectorIdentifierUri != null)
            {
                var sectorIdentifierUris =
                    await GetSectorIdentifierUris(newClient.SectorIdentifierUri, cancellationToken).ConfigureAwait(false);
                if (sectorIdentifierUris.Any(
                    sectorIdentifierUri => !newClient.RedirectionUrls.Contains(sectorIdentifierUri)))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidClientMetaData,
                        Strings.OneOrMoreSectorIdentifierUriIsNotARedirectUri);
                }
            }

            if (!string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseEnc))
            {
                if (string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseAlg))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidClientMetaData,
                        Strings.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
                }
            }

            if (!string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseEnc))
            {
                if (string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseAlg))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidClientMetaData,
                        Strings.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
                }
            }

            if (!string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionEnc))
            {
                if (string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionAlg))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidClientMetaData,
                        Strings.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
                }
            }

            ValidateNotMandatoryUri(newClient.InitiateLoginUri, "initiate_login_uri", true);

            if (newClient.RequestUris == null)
            {
                newClient.RequestUris = Array.Empty<Uri>();
            }

            if (newClient.RequestUris.Any(requestUri => !requestUri.IsAbsoluteUri))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    Strings.OneOfTheRequestUriIsNotValid);
            }

            var client = new Client
            {
                ClientId = Id.Create()
            };

            client.ClientName = string.IsNullOrWhiteSpace(newClient.ClientName)
                ? "Unnamed_" + client.ClientId
                : newClient.ClientName;

            client.TokenLifetime = newClient.TokenLifetime;
            client.ApplicationType = newClient.ApplicationType;
            client.ClientUri = newClient.ClientUri;
            client.Contacts = newClient.Contacts;
            client.DefaultAcrValues = newClient.DefaultAcrValues;

            // If omitted then the default value is authorization code grant type
            client.GrantTypes = newClient.GrantTypes == null || !newClient.GrantTypes.Any()
                ? new[] {GrantTypes.AuthorizationCode}
                : newClient.GrantTypes;

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
                    Strings.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
            }

            client.IdTokenSignedResponseAlg = !string.IsNullOrWhiteSpace(newClient.IdTokenSignedResponseAlg)
                ? newClient.IdTokenSignedResponseAlg
                : SecurityAlgorithms.RsaSha256;

            client.InitiateLoginUri = newClient.InitiateLoginUri;

            client.JsonWebKeys = newClient.JsonWebKeys ?? new JsonWebKeySet();
            //client.JwksUri = newClient.JwksUri;
            //client.LogoUri = newClient.LogoUri;
            client.PolicyUri = newClient.PolicyUri;
            client.PostLogoutRedirectUris = newClient.PostLogoutRedirectUris;

            if (newClient.AllowedScopes == null)
            {
                newClient.AllowedScopes = Array.Empty<string>();
            }

            var scopes = await _scopeRepository.SearchByNames(CancellationToken.None, newClient.AllowedScopes)
                .ConfigureAwait(false);
            if (scopes.Length != newClient.AllowedScopes.Length)
            {
                var enumerable = newClient.AllowedScopes.Except(scopes.Select(x => x.Name));
                throw new SimpleAuthException(
                    ErrorCodes.InvalidScope,
                    $"Unknown scopes: {string.Join(",", enumerable)}");
            }

            client.AllowedScopes = newClient.AllowedScopes.ToArray();

            if (newClient.RedirectionUrls == null || newClient.RedirectionUrls.Length == 0)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRedirectUri,
                    string.Format(Strings.MissingParameter, "redirect_uris"));
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
                            string.Format(Strings.TheRedirectUrlIsNotValid, redirectUri));
                    }

                    if (!string.IsNullOrWhiteSpace(redirectUri.Fragment))
                    {
                        throw new SimpleAuthException(
                            ErrorCodes.InvalidRedirectUri,
                            string.Format(Strings.TheRedirectUrlCannotContainsFragment, redirectUri));
                    }

                    client.RedirectionUrls = client.RedirectionUrls.Add(redirectUri);
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
                            string.Format(Strings.TheRedirectUrlIsNotValid, redirectUri));
                    }

                    client.RedirectionUrls = client.RedirectionUrls.Add(redirectUri);
                }
            }

            client.RequestObjectEncryptionAlg = newClient.RequestObjectEncryptionAlg;
            client.RequestObjectEncryptionEnc = newClient.RequestObjectEncryptionEnc;
            client.RequestObjectSigningAlg = newClient.RequestObjectSigningAlg;
            client.RequestUris = newClient.RequestUris;
            client.RequireAuthTime = newClient.RequireAuthTime;
            client.RequirePkce = newClient.RequirePkce;

            // If omitted then the default value is authorization code response type
            client.ResponseTypes = newClient.ResponseTypes?.Any() != true ? new[] { ResponseTypeNames.Code } : newClient.ResponseTypes;
            client.Secrets = newClient.Secrets;
            client.SectorIdentifierUri = newClient.SectorIdentifierUri;
            //client.SubjectType = newClient.SubjectType;

            if (client.Secrets?.Length == 0
                && client.TokenEndPointAuthMethod != TokenEndPointAuthenticationMethods.PrivateKeyJwt)
            {
                client.Secrets = new[]
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = Id.Create()
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

        private async Task<IReadOnlyCollection<Uri>> GetSectorIdentifierUris(Uri sectorIdentifierUri, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = _httpClient.GetAsync(sectorIdentifierUri, cancellationToken).Result;
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return _urlReader(result);
                //result.DeserializeWithJavascript<List<string>>().Select(x => new Uri(x)).ToList();
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    Strings.TheSectorIdentifierUrisCannotBeRetrieved,
                    ex);
            }
        }

        private static void ValidateNotMandatoryUri(Uri uri, string newClientName, bool checkSchemeIsHttps = false)
        {
            if (uri == null)
            {
                return;
            }

            if (!uri.IsAbsoluteUri || !Uri.IsWellFormedUriString(uri.AbsoluteUri, UriKind.Absolute))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    string.Format(Strings.ParameterIsNotCorrect, newClientName));
            }

            if (checkSchemeIsHttps && uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClientMetaData,
                    string.Format(Strings.ParameterIsNotCorrect, newClientName));
            }
        }
    }
}
