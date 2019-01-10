namespace SimpleAuth.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Runtime.Serialization;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Api.Token;
    using Errors;
    using Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Shared;
    using Shared.Models;
    using Shared.Requests;
    using Shared.Responses;
    using Shared.Serializers;
    using GrantTypes = Shared.Requests.GrantTypes;

    [Route(UmaConstants.RouteValues.Token)]
    public class TokenController : Controller
    {
        private readonly ITokenActions _tokenActions;
        private readonly IUmaTokenActions _umaTokenActions;

        public TokenController(ITokenActions tokenActions, IUmaTokenActions umaTokenActions)
        {
            _tokenActions = tokenActions;
            _umaTokenActions = umaTokenActions;
        }

        [HttpPost]
        public async Task<IActionResult> PostToken([FromForm]TokenRequest tokenRequest)
        {
            var certificate = GetCertificate();
            if (tokenRequest.grant_type == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = ErrorCodes.InvalidRequestCode,
                    ErrorDescription = string.Format(ErrorDescriptions.MissingParameter, RequestTokenNames.GrantType)
                });
            }

            //if (Request.Form == null)
            //{
            //    return StatusCode(
            //        (int)HttpStatusCode.BadRequest,
            //        new ErrorResponse
            //        {
            //            Error = ErrorCodes.InvalidRequestCode,
            //            ErrorDescription = "no parameter in body request"
            //        });
            //}

            //var serializer = new ParamSerializer();
            //var tokenRequest = serializer.Deserialize<TokenRequest>(Request.Form.Select(x => new KeyValuePair<string, string[]>(x.Key, x.Value)));
            GrantedToken result = null;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader[0];
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Length == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            switch (tokenRequest.grant_type)
            {
                case GrantTypes.password:
                    var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
                    result = await _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(resourceOwnerParameter, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
                    break;
                case GrantTypes.authorization_code:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    result = await _tokenActions.GetTokenByAuthorizationCodeGrantType(authCodeParameter, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
                    break;
                case GrantTypes.refresh_token:
                    var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                    result = await _tokenActions.GetTokenByRefreshTokenGrantType(refreshTokenParameter, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
                    break;
                case GrantTypes.client_credentials:
                    var clientCredentialsParameter = tokenRequest.ToClientCredentialsGrantTypeParameter();
                    result = await _tokenActions.GetTokenByClientCredentialsGrantType(clientCredentialsParameter, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
                    break;
                case GrantTypes.uma_ticket:
                    var tokenIdParameter = tokenRequest.ToTokenIdGrantTypeParameter();
                    result = await _umaTokenActions.GetTokenByTicketId(tokenIdParameter, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
                    break;
                case GrantTypes.validate_bearer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new OkObjectResult(result.ToDto());
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> PostRevoke([FromForm]RevocationRequest revocationRequest)
        {
            //if (Request.Form == null)
            //{
            //    throw new ArgumentNullException(nameof(Request.Form));
            //}

            //var serializer = new ParamSerializer();
            //var revocationRequest = serializer.Deserialize<RevocationRequest>(Request.Form.Select(x => new KeyValuePair<string, string[]>(x.Key, x.Value)));
            // 1. Fetch the authorization header
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Length == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            // 2. Revoke the token
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            await _tokenActions.RevokeToken(revocationRequest.ToParameter(), authenticationHeaderValue, GetCertificate(), issuerName).ConfigureAwait(false);
            return new OkResult();
        }

        private X509Certificate2 GetCertificate()
        {
            const string headerName = "X-ARR-ClientCert";
            var header = Request.Headers.FirstOrDefault(h => h.Key == headerName);
            if (header.Equals(default(KeyValuePair<string, StringValues>)))
            {
                return null;
            }

            try
            {
                var encoded = Convert.FromBase64String(header.Value);
                return new X509Certificate2(encoded);
            }
            catch
            {
                return null;
            }
        }
    }
}
