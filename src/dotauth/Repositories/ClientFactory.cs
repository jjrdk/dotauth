namespace DotAuth.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

internal sealed class ClientFactory
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
            _logger.LogError("{Error}", e.Details.Detail);
            return new Option<Client>.Error(e.Details, e.State);
        }
        result = ValidateNotMandatoryUri(newClient.TosUri, "tos_uri");
        if (result is Option.Error e2)
        {
            _logger.LogError("{Error}", e2.Details.Detail);
            return new Option<Client>.Error(e2.Details, e2.State);
        }
        result = ValidateNotMandatoryUri(newClient.SectorIdentifierUri, "sector_identifier_uri", true);
        if (result is Option.Error e3)
        {
            _logger.LogError("{Error}", e3.Details.Detail);
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
                _logger.LogError("{Error}", Strings.OneOrMoreSectorIdentifierUriIsNotARedirectUri);
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
            _logger.LogError("{Error}", Strings.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
            return new Option<Client>.Error(new ErrorDetails
            {
                Title = ErrorCodes.InvalidClientMetaData,
                Detail = Strings.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified,
                Status = HttpStatusCode.BadRequest
            });
        }

        if (!string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseEnc) && string.IsNullOrWhiteSpace(newClient.UserInfoEncryptedResponseAlg))
        {
            _logger.LogError("{Error}", Strings.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
            return new Option<Client>.Error(new ErrorDetails
            {
                Title = ErrorCodes.InvalidClientMetaData,
                Detail = Strings.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified,
                Status = HttpStatusCode.BadRequest
            });
        }

        if (!string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionEnc) && string.IsNullOrWhiteSpace(newClient.RequestObjectEncryptionAlg))
        {
            _logger.LogError("{Error}", Strings.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
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
            _logger.LogError("{Error}", message);
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

        var client = new Client
        {
            ClientId = updateId ? Id.Create() : newClient.ClientId
        };

        client.ClientName = string.IsNullOrWhiteSpace(newClient.ClientName)
            ? $"Unnamed_{client.ClientId}"
            : newClient.ClientName;

        client.TokenLifetime = newClient.TokenLifetime;
        client.ApplicationType = newClient.ApplicationType;
        client.ClientUri = newClient.ClientUri;
        client.Contacts = newClient.Contacts;
        client.DefaultAcrValues = newClient.DefaultAcrValues;

        // If omitted then the default value is authorization code response type
        client.ResponseTypes = newClient.ResponseTypes.Length == 0 ? [ResponseTypeNames.Code] : newClient.ResponseTypes;
        client.SectorIdentifierUri = newClient.SectorIdentifierUri;
        client.TokenEndPointAuthMethod = newClient.TokenEndPointAuthMethod;
        client.TokenEndPointAuthSigningAlg = newClient.TokenEndPointAuthSigningAlg;
        client.TosUri = newClient.TosUri;
        client.UserInfoEncryptedResponseAlg = newClient.UserInfoEncryptedResponseAlg;
        client.UserInfoEncryptedResponseEnc = newClient.UserInfoEncryptedResponseEnc;

        client.Secrets = newClient.Secrets.Length switch
        {
            0 when client.TokenEndPointAuthMethod != TokenEndPointAuthenticationMethods.PrivateKeyJwt =>
            [
                new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = Id.Create()}
            ],
            > 0 => newClient.Secrets.Select(
                    secret => secret.Type == ClientSecretTypes.SharedSecret
                        ? new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = Id.Create() }
                        : secret)
                .ToArray(),
            _ => client.Secrets
        };

        // If omitted then the default value is authorization code grant type
        client.GrantTypes = newClient.GrantTypes.Length == 0
            ? [GrantTypes.AuthorizationCode]
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
            _logger.LogError("{Error}", message);
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
        client.JwksUri = newClient.JwksUri;
        client.PolicyUri = newClient.PolicyUri;
        client.PostLogoutRedirectUris = newClient.PostLogoutRedirectUris;

        // G6: Validate client key material for the JWT-based client authentication methods.
        var authMethodError = ValidateKeyMaterialForClientAuthMethod(client);
        if (authMethodError is not null)
         {
             return new Option<Client>.Error(authMethodError.Details);
         }

        //newClient.AllowedScopes ??= Array.Empty<string>();

        var scopes = await _scopeRepository.SearchByNames(CancellationToken.None, newClient.AllowedScopes)
            .ConfigureAwait(false);
        if (scopes.Length != newClient.AllowedScopes.Length)
        {
            var enumerable = newClient.AllowedScopes.Except(scopes.Select(x => x.Name));
            var message = $"Unknown scopes: {string.Join(",", enumerable)}";
            _logger.LogError("{Error}", message);
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
                    _logger.LogError("{Error}", message);
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
                    _logger.LogError("{Error}", message);
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
                    _logger.LogError("{Error}", message);
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
            _logger.LogError("{Error}", Strings.TheSectorIdentifierUrisCannotBeRetrieved);
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

        /// <summary>
        /// G6: Validates that a <c>private_key_jwt</c> or <c>client_secret_jwt</c> client is
        /// registered with usable, well-formed key material.
        /// </summary>
         private Option<Client>.Error? ValidateKeyMaterialForClientAuthMethod(Client client)
        {
        var method = client.TokenEndPointAuthMethod;
        if (method != TokenEndPointAuthenticationMethods.PrivateKeyJwt
            && method != TokenEndPointAuthenticationMethods.ClientSecretJwt)
        {
          return null;
        }

        if (method == TokenEndPointAuthenticationMethods.ClientSecretJwt)
        {
           // client_secret_jwt signs the assertion with the client's shared secret.
          if (client.Secrets.All(s => s.Type != ClientSecretTypes.SharedSecret
                          || string.IsNullOrWhiteSpace(s.Value)))
            {
               _logger.LogError("{Error}", "client_secret_jwt requires a shared secret.");
              return new Option<Client>.Error(new ErrorDetails
                 {
                   Title = ErrorCodes.InvalidClientMetaData,
                   Detail = "client_secret_jwt requires a non-empty shared secret.",
                   Status = HttpStatusCode.BadRequest
                 });
            }

           if (!string.IsNullOrWhiteSpace(client.TokenEndPointAuthSigningAlg)
               && !CoreConstants.Supported.ClientSecretJwtSigningAlgorithms.Any(x =>
                   string.Equals(x, client.TokenEndPointAuthSigningAlg, StringComparison.OrdinalIgnoreCase)))
           {
               _logger.LogError("{Error}", "client_secret_jwt requires an HS256/384/512 token_endpoint_auth_signing_alg.");
               return new Option<Client>.Error(new ErrorDetails
               {
                   Title = ErrorCodes.InvalidClientMetaData,
                   Detail = "client_secret_jwt requires token_endpoint_auth_signing_alg to be one of HS256, HS384, or HS512.",
                   Status = HttpStatusCode.BadRequest
               });
           }

          return null;
        }

        // private_key_jwt: at least one of 'jwks' or 'jwks_uri' must be present.
        var hasEmbeddedKeys = client.JsonWebKeys != null && client.JsonWebKeys.Keys.Count > 0;
        var hasJwksUri = client.JwksUri != null;
        if (!hasEmbeddedKeys && !hasJwksUri)
        {
           _logger.LogError("{Error}", "private_key_jwt requires at least one of 'jwks' or 'jwks_uri'.");
          return new Option<Client>.Error(new ErrorDetails
             {
              Title = ErrorCodes.InvalidClientMetaData,
              Detail = "private_key_jwt requires at least one of 'jwks' or 'jwks_uri' with key material.",
              Status = HttpStatusCode.BadRequest
             });
        }

        if (string.IsNullOrWhiteSpace(client.TokenEndPointAuthSigningAlg)
            || !CoreConstants.Supported.PrivateKeyJwtSigningAlgorithms.Any(x =>
                string.Equals(x, client.TokenEndPointAuthSigningAlg, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogError("{Error}", "private_key_jwt requires an explicit supported token_endpoint_auth_signing_alg.");
            return new Option<Client>.Error(new ErrorDetails
            {
                Title = ErrorCodes.InvalidClientMetaData,
                Detail = "private_key_jwt requires a supported token_endpoint_auth_signing_alg at registration time.",
                Status = HttpStatusCode.BadRequest
            });
        }

        // Validate embedded key material (type, kid, alg) when present.
        if (client.JsonWebKeys != null)
         {
          var validationError = ValidateSignatureKeys(client.JsonWebKeys, method);
          if (validationError != null)
            {
               return new Option<Client>.Error(new ErrorDetails
                {
                  Title = ErrorCodes.InvalidClientMetaData,
                  Detail = validationError,
                  Status = HttpStatusCode.BadRequest
                });
            }
        }

        return null;
        }

        // Validates that at least one signature key is usable: has a non-empty kid and an
        // algorithm in the OP-supported set for the given method. Symmetric keys are rejected
        // for private_key_jwt.
        private static string? ValidateSignatureKeys(JsonWebKeySet jwks, string method)
        {
        var signatureKeys = jwks.Keys.Where(k => k.Use != JsonWebKeyUseNames.Enc).ToList();
        if (signatureKeys.Count == 0)
         {
           return "no signature key found in the supplied 'jwks'.";
         }

        // At least one key must carry a usable key identifier.
        var hasKid = signatureKeys.Any(k => !string.IsNullOrWhiteSpace(k.Kid));
        if (!hasKid)
         {
           return "at least one signature key must include a 'kid'.";
        }

        var asymmetricMethod = method == TokenEndPointAuthenticationMethods.PrivateKeyJwt;
        if (asymmetricMethod)
         {
           // private_key_jwt MUST be asymmetric — reject a symmetric (oct) key.
           var hasAsymmetric = signatureKeys.Any(k =>
               string.IsNullOrWhiteSpace(k.Kty)
               || (k.Kty != "oct" && !IsSymmetricAlg(k.Alg)));
          if (!hasAsymmetric)
            {
              return "private_key_jwt requires an asymmetric (RSA/EC) signature key, not a symmetric key.";
            }
        }

        // At least one key must declare an OP-supported algorithm.
        var hasSupportedAlg = signatureKeys.Any(k =>
           IsAlgSupportedForMethod(k.Alg, method));
        if (!hasSupportedAlg)
        {
         return "no signature key uses an OP-supported algorithm.";
        }

        return null;
        }

        private static bool IsSymmetricAlg(string? alg)
        {
        return !string.IsNullOrWhiteSpace(alg)
           && alg.StartsWith("HS", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAlgSupportedForMethod(string? alg, string method)
        {
         if (string.IsNullOrWhiteSpace(alg))
          {
           // If alg is omitted, fall back to the method's default supported algorithm.
            return method == TokenEndPointAuthenticationMethods.ClientSecretJwt
               ? CoreConstants.Supported.ClientSecretJwtSigningAlgorithms.Length > 0
               : CoreConstants.Supported.PrivateKeyJwtSigningAlgorithms.Length > 0;
          }

        var supported = method == TokenEndPointAuthenticationMethods.ClientSecretJwt
          ? CoreConstants.Supported.ClientSecretJwtSigningAlgorithms
          : CoreConstants.Supported.PrivateKeyJwtSigningAlgorithms;

        return supported.Any(a => string.Equals(a, alg, StringComparison.OrdinalIgnoreCase));
        }
        }
