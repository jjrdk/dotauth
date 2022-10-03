namespace DotAuth.Api.Introspection;

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Responses;

internal sealed class UmaIntrospectionAction
{
    private readonly ITokenStore _tokenStore;

    public UmaIntrospectionAction(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public async Task<Option<UmaIntrospectionResponse>> Execute(
        IntrospectionParameter introspectionParameter,
        CancellationToken cancellationToken)
    {
        var introspectionParameterToken = introspectionParameter.Token;
        if (string.IsNullOrWhiteSpace(introspectionParameterToken))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidToken,
                Detail = Strings.TheTokenDoesntExist
            };
        }
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
                .GetAccessToken(introspectionParameterToken, cancellationToken)
                .ConfigureAwait(false),
            CoreConstants.StandardTokenTypeHintNames.RefreshToken => await _tokenStore
                .GetRefreshToken(introspectionParameterToken, cancellationToken)
                .ConfigureAwait(false),
            _ => null
        };

        // 5. Return an error if there's no granted token
        if (grantedToken == null)
        {
            return new ErrorDetails { Title = ErrorCodes.InvalidGrant, Detail = Strings.TheTokenIsNotValid };
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
                grantedToken.IdTokenPayLoad?.GetClaimValue(OpenIdClaimTypes.Subject);
            if (!string.IsNullOrWhiteSpace(subject))
            {
                result = result with { Subject = subject };
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

            result = result with
            {
                Audience = string.Join(" ", audiencesArr),
                IssuedAt = grantedToken.IdTokenPayLoad.Iat ?? 0,
                Issuer = grantedToken.IdTokenPayLoad.Iss
            };
            if (!string.IsNullOrWhiteSpace(subject))
            {
                result = result with { Subject = subject };
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                result = result with { UserName = userName };
            }
        }

        // 8. Based on the expiration date disable OR enable the introspection resultKind
        var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
        result = result with { Active = DateTimeOffset.UtcNow < expirationDateTime };

        return result;
    }
}