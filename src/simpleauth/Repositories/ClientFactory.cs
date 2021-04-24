namespace SimpleAuth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal class ClientFactory
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IScopeStore _scopeRepository;
        private readonly Func<string, Uri[]> _urlReader;
        private readonly ILogger _logger;

        public ClientFactory(IHttpClientFactory httpClient, IScopeStore scopeRepository, Func<string, Uri[]> urlReader, ILogger logger)
        {
            _httpClient = httpClient;
            _scopeRepository = scopeRepository;
            _urlReader = urlReader;
            _logger = logger;
        }

        public async Task<Option<Client>> Build(Client newClient, bool updateId = true, CancellationToken cancellationToken = default)
        {
            var result = ValidateNotMandatoryUri(newClient.ClientUri, "client_uri");
            if (result is Option.Error e)
            {
                _logger.LogError(e.Details.Detail);
                return new Option<Client>.Error(e.Details, e.State);
            }
            result = ValidateNotMandatoryUri(newClient.TosUri, "tos_uri");
            if (result is Option.Error e2)
            {
                _logger.LogError(e2.Details.Detail);
                return new Option<Client>.Error(e2.Details, e2.State);
            }
            result = ValidateNotMandatoryUri(newClient.SectorIdentifierUri, "sector_identifier_uri", true);
            if (result is Option.Error e3)
            {
                _logger.LogError(e3.Details.Detail);
                return new Option<Client>.Error(e3.Details, e3.State);
            }
            // Based on the RFC : http://openid.net/specs/openid-connect-registration-1_0.html#SectorIdentifierValidation validate the sector_identifier_uri
            if (newClient.SectorIdentifierUri != null)
            {
                var sectorIdentifierUrisOption =
                    await GetSectorIdentifierUris(newClient.SectorIdentifierUri, cancellationToken).ConfigureAwait(false);
                if (sectorIdentifierUrisOption is Option<IReadOnlyCollection<Uri>>.Error error)
                {
                    return new Option<Client>.Error(error.Details, error.State);
                }

                var sectorIdentifierUris = ((Option<IReadOnlyCollection<Uri>>.Result)sectorIdentifierUrisOption).Item;
                if (sectorIdentifierUris.Any(
                    sectorIdentifierUri => !newClient.RedirectionUrls.Contains(sectorIdentifierUri)))
                {
                    _logger.LogError(Strings.OneOrMoreSectorIdentifierUriIsNotARedirectUri);
                    return new Option<Client>.Error(new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidClientMetaData,
                        Detail = Strings.OneOrMoreSectorIdentifierUriIsNotARedirectUri,
                        Status = HttpStatusCode.BadRequest
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseEnc) && string.IsNullOrWhiteSpace(newClient.IdTokenEncryptedResponseAlg))
            {
                _logger.LogError(Strings.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
                return new Option<Client>.Error(new ErrorDetails
                {
                    Title = ErrorCodes.InvalidClientMetaData,
                    Detail = Strings.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified,
                    Status = HttpStatusCode.BadRequest
                });
            }

            if (!string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseEnc) && string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseAlg))
            {
                _logger.LogError(Strings.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
                return new Option<Client>.Error(new ErrorDetails
                {
                    Title = ErrorCodes.InvalidClientMetaData,
                    Detail = Strings.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified,
                    Status = HttpStatusCode.BadRequest
                });
            }

            if (!string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionEnc) && string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionAlg))
            {
                _logger.LogError(Strings.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
                return new Option<Client>.Error(new ErrorDetails
                {
                    Title = ErrorCodes.InvalidClientMetaData,
                    Detail = Strings.TheParameterRequestObjectEncryptionAlgMustBeSpecified,
                    Status = HttpStatusCode.BadRequest
                });
            }

            if (newClient.RedirectionUrls.Length == 0)
            {
                var message = string.Format(Strings.MissingParameter, "redirect_uris");
                _logger.LogError(message);
                return new Option<Client>.Error(new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRedirectUri,
                    Detail = message,
                    Status = HttpStatusCode.BadRequest
                });
            }

            result = ValidateNotMandatoryUri(newClient.InitiateLoginUri, "initiate_login_uri", true);
            if (result is Option.Error e4)
            {
                return new Option<Client>.Error(e4.Details, e4.State);
            }
            if (newClient.RequestUris.Any(requestUri => !requestUri.IsAbsoluteUri))
            {
                var message = Strings.OneOfTheRequestUriIsNotValid;
                _logger.LogError(message);
                return new Option<Client>.Error(new ErrorDetails
                {
                    Title = ErrorCodes.InvalidClientMetaData,
                    Detail = message,
                    Status = HttpStatusCode.BadRequest
                });
            }

            var client = new Client
            {
                ClientId = updateId ? Id.Create() : newClient.ClientId
            };

            client.ClientName = string.IsNullOrWhiteSpace(newClient.ClientName)
                ? "Unnamed_" + client.ClientId
                : newClient.ClientName;

            client.TokenLifetime = newClient.TokenLifetime;
            client.ApplicationType = newClient.ApplicationType;
            client.ClientUri = newClient.ClientUri;
            client.Contacts = newClient.Contacts;
            client.DefaultAcrValues = newClient.DefaultAcrValues;

            // If omitted then the default value is authorization code response type
            client.ResponseTypes = newClient.ResponseTypes.Length == 0 ? new[] { ResponseTypeNames.Code } : newClient.ResponseTypes;
            client.SectorIdentifierUri = newClient.SectorIdentifierUri;
            client.TokenEndPointAuthMethod = newClient.TokenEndPointAuthMethod;
            client.TokenEndPointAuthSigningAlg = newClient.TokenEndPointAuthSigningAlg;
            client.TosUri = newClient.TosUri;
            client.UserInfoEncryptedResponseAlg = newClient.UserInfoEncryptedResponseAlg;
            client.UserInfoEncryptedResponseEnc = newClient.UserInfoEncryptedResponseEnc;

            client.Secrets = newClient.Secrets.Length switch
            {
                0 when client.TokenEndPointAuthMethod != TokenEndPointAuthenticationMethods.PrivateKeyJwt => new[]
                {
                    new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = Id.Create()}
                },
                > 0 => newClient.Secrets.Select(
                        secret => secret.Type == ClientSecretTypes.SharedSecret
                            ? new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = Id.Create()}
                            : secret)
                    .ToArray(),
                _ => client.Secrets
            };

            // If omitted then the default value is authorization code grant type
            client.GrantTypes = newClient.GrantTypes.Length == 0
                ? new[] { GrantTypes.AuthorizationCode }
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
                var message = Strings.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified;
                _logger.LogError(message);
                return new Option<Client>.Error(new ErrorDetails
                {
                    Title = ErrorCodes.InvalidClientMetaData,
                    Detail = message,
                    Status = HttpStatusCode.BadRequest
                });
            }

            client.IdTokenSignedResponseAlg = !string.IsNullOrWhiteSpace(newClient.IdTokenSignedResponseAlg)
                ? newClient.IdTokenSignedResponseAlg
                : SecurityAlgorithms.RsaSha256;

            client.InitiateLoginUri = newClient.InitiateLoginUri;

            client.JsonWebKeys = newClient.JsonWebKeys;
            client.PolicyUri = newClient.PolicyUri;
            client.PostLogoutRedirectUris = newClient.PostLogoutRedirectUris;

            //newClient.AllowedScopes ??= Array.Empty<string>();

            var scopes = await _scopeRepository.SearchByNames(CancellationToken.None, newClient.AllowedScopes)
                .ConfigureAwait(false);
            if (scopes.Length != newClient.AllowedScopes.Length)
            {
                var enumerable = newClient.AllowedScopes.Except(scopes.Select(x => x.Name));
                var message = $"Unknown scopes: {string.Join(",", enumerable)}";
                _logger.LogError(message);
                return new Option<Client>.Error(new ErrorDetails
                {
                    Title = ErrorCodes.InvalidScope,
                    Detail = message,
                    Status = HttpStatusCode.BadRequest
                });
            }

            client.AllowedScopes = newClient.AllowedScopes.ToArray();

            // Check the newClients when the application type is web
            if (client.ApplicationType == ApplicationTypes.Web)
            {
                foreach (var redirectUri in newClient.RedirectionUrls)
                {
                    if (!redirectUri.IsAbsoluteUri || !Uri.IsWellFormedUriString(redirectUri.AbsoluteUri, UriKind.Absolute))
                    {
                        var message = string.Format(Strings.TheRedirectUrlIsNotValid, redirectUri);
                        _logger.LogError(message);
                        return new Option<Client>.Error(new ErrorDetails
                        {
                            Title = ErrorCodes.InvalidRedirectUri,
                            Detail = message,
                            Status = HttpStatusCode.BadRequest
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(redirectUri.Fragment))
                    {
                        var message = string.Format(Strings.TheRedirectUrlCannotContainsFragment, redirectUri);
                        _logger.LogError(message);
                        return new Option<Client>.Error(
                            new ErrorDetails
                            {
                                Title = ErrorCodes.InvalidRedirectUri,
                                Detail = message,
                                Status = HttpStatusCode.BadRequest
                            });
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
                        var message = string.Format(Strings.TheRedirectUrlIsNotValid, redirectUri);
                        _logger.LogError(message);
                        return new Option<Client>.Error(
                            new ErrorDetails
                            {
                                Title = ErrorCodes.InvalidRedirectUri,
                                Detail = message,
                                Status = HttpStatusCode.BadRequest
                            });
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

            return new Option<Client>.Result(client);
        }

        private async Task<Option<IReadOnlyCollection<Uri>>> GetSectorIdentifierUris(Uri sectorIdentifierUri, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _httpClient.CreateClient();
                var response = client.GetAsync(sectorIdentifierUri, cancellationToken).Result;
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return new Option<IReadOnlyCollection<Uri>>.Result(_urlReader(result));
                //result.DeserializeWithJavascript<List<string>>().Select(x => new Uri(x)).ToList();
            }
            catch
            {
                _logger.LogError(Strings.TheSectorIdentifierUrisCannotBeRetrieved);
                return new Option<IReadOnlyCollection<Uri>>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidClientMetaData,
                        Detail = Strings.TheSectorIdentifierUrisCannotBeRetrieved,
                        Status = HttpStatusCode.BadRequest
                    });
            }
        }

        private static Option ValidateNotMandatoryUri(Uri? uri, string parameter, bool checkSchemeIsHttps = false)
        {
            if (uri == null)
            {
                return new Option.Success();
            }

            if (!uri.IsAbsoluteUri || !Uri.IsWellFormedUriString(uri.AbsoluteUri, UriKind.Absolute))
            {
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidClientMetaData,
                        Detail = string.Format(Strings.ParameterIsNotCorrect, parameter),
                        Status = HttpStatusCode.BadRequest
                    });
            }

            if (checkSchemeIsHttps && uri.Scheme != Uri.UriSchemeHttps)
            {
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidClientMetaData,
                        Detail = string.Format(Strings.ParameterIsNotCorrect, parameter),
                        Status = HttpStatusCode.BadRequest
                    });
            }

            return new Option.Success();
        }
    }
}
