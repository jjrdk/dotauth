namespace SimpleAuth.Api.Introspection
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Parameters;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Responses;

    internal class UmaIntrospectionAction
    {
        private readonly ITokenStore _tokenStore;

        public UmaIntrospectionAction(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        public async Task<GenericResponse<UmaIntrospectionResponse>> Execute(
            IntrospectionParameter introspectionParameter,
            CancellationToken cancellationToken)
        {
            // Read this RFC for more information - https://docs.kantarainitiative.org/uma/wg/rec-oauth-uma-federated-authz-2.0.html#introspection-endpoint

            // 3. Retrieve the token type hint
            var tokenTypeHint = CoreConstants.StandardTokenTypeHintNames.AccessToken;
            if (CoreConstants.AllStandardTokenTypeHintNames.Contains(introspectionParameter.TokenTypeHint))
            {
                tokenTypeHint = introspectionParameter.TokenTypeHint;
            }

            // 4. Trying to fetch the information about the access_token  || refresh_token
            var grantedToken = tokenTypeHint switch
            {
                CoreConstants.StandardTokenTypeHintNames.AccessToken => await _tokenStore
                    .GetAccessToken(introspectionParameter.Token, cancellationToken)
                    .ConfigureAwait(false),
                CoreConstants.StandardTokenTypeHintNames.RefreshToken => await _tokenStore
                    .GetRefreshToken(introspectionParameter.Token, cancellationToken)
                    .ConfigureAwait(false),
                _ => null
            };

            // 5. Return an error if there's no granted token
            if (grantedToken == null)
            {
                return new GenericResponse<UmaIntrospectionResponse>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidGrant,
                        Detail = ErrorDescriptions.TheTokenIsNotValid
                    }
                };
            }

            // 6. Fill-in parameters
            //// default : Specify the other parameters : NBF & JTI
            var result = new UmaIntrospectionResponse
            {
                //Scope = grantedToken.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                ClientId = grantedToken.ClientId,
                Expiration = grantedToken.ExpiresIn,
                TokenType = grantedToken.TokenType
            };

            if (grantedToken.UserInfoPayLoad != null)
            {
                var subject =
                    grantedToken.IdTokenPayLoad.GetClaimValue(OpenIdClaimTypes.Subject);
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    result.Subject = subject;
                }
            }

            // 7. Fill-in the other parameters
            if (grantedToken.IdTokenPayLoad != null)
            {
                var audiencesArr = grantedToken.IdTokenPayLoad.GetArrayValue(StandardClaimNames.Audiences);
                var subject =
                    grantedToken.IdTokenPayLoad.GetClaimValue(OpenIdClaimTypes.Subject);
                var userName =
                    grantedToken.IdTokenPayLoad.GetClaimValue(OpenIdClaimTypes.Name);

                result.Audience = string.Join(" ", audiencesArr);
                result.IssuedAt = grantedToken.IdTokenPayLoad.Iat ?? 0;
                result.Issuer = grantedToken.IdTokenPayLoad.Iss;
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    result.Subject = subject;
                }

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    result.UserName = userName;
                }
            }

            // 8. Based on the expiration date disable OR enable the introspection resultKind
            var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
            result.Active = DateTimeOffset.UtcNow < expirationDateTime;

            return new GenericResponse<UmaIntrospectionResponse>
            {
                Content = result,
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}