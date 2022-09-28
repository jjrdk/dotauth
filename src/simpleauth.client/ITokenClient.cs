namespace SimpleAuth.Client;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Requests;
using SimpleAuth.Shared.Responses;

/// <summary>
/// Defines the token client interface.
/// </summary>
public interface ITokenClient
{
    /// <summary>
    /// Executes the specified introspection request.
    /// </summary>
    /// <param name="introspectionRequest">The introspection request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<OauthIntrospectionResponse>> Introspect(
        IntrospectionRequest introspectionRequest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the token.
    /// </summary>
    /// <param name="tokenRequest">The token request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<GrantedTokenResponse>> GetToken(TokenRequest tokenRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the authorization.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>The <see cref="Uri"/> to execute the authorization flow.</returns>
    /// <exception cref="ArgumentNullException">request</exception>
    Task<Option<Uri>> GetAuthorization(
        AuthorizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a device authorization
    /// </summary>
    /// <param name="request">The authorization request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>The <see cref="DeviceAuthorizationResponse"/> with information about how to execute the authorization flow.</returns>
    Task<Option<DeviceAuthorizationResponse>> GetAuthorization(
        DeviceAuthorizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the specified request URL.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option> RequestSms(
        ConfirmationCodeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the token.
    /// </summary>
    /// <param name="revokeTokenRequest">The revoke token request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option> RevokeToken(
        RevokeTokenRequest revokeTokenRequest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the specified user info based on the configuration URL and access token.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="inBody">if set to <c>true</c> [in body].</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    /// configurationUrl
    /// or
    /// accessToken
    /// </exception>
    /// <exception cref="ArgumentException"></exception>
    Task<Option<JwtPayload>> GetUserInfo(
        string accessToken,
        bool inBody = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the public web keys.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>The public <see cref="JsonWebKeySet"/> as a <see cref="Task{TResult}"/>.</returns>
    Task<JsonWebKeySet> GetJwks(CancellationToken cancellationToken = default);
}