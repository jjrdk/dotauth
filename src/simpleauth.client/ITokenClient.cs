namespace SimpleAuth.Client
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the token client interface.
    /// </summary>
    public interface ITokenClient
    {
        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <returns></returns>
        Task<GenericResponse<GrantedTokenResponse>> GetToken(TokenRequest tokenRequest);

        /// <summary>
        /// Sends the specified request URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<GenericResponse<object>> RequestSms(
            ConfirmationCodeRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the token.
        /// </summary>
        /// <param name="revokeTokenRequest">The revoke token request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<GenericResponse<object>> RevokeToken(
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
        Task<GenericResponse<JwtPayload>> GetUserInfo(
            string accessToken,
            bool inBody = false,
            CancellationToken cancellationToken = default);
    }
}