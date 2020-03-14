namespace SimpleAuth.ResourceServer
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the token cache interface
    /// </summary>
    public interface ITokenCache
    {
        /// <summary>
        /// Gets the token as an async operation.
        /// </summary>
        /// <param name="scopes">The scopes to include in the token request.</param>
        /// <returns>The <see cref="GrantedTokenResponse"/> as a <see cref="Task{TResult}"/>.</returns>
        Task<GrantedTokenResponse> GetToken(params string[] scopes);

        /// <summary>
        /// Gets the JSON Web Key set.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation.</param>
        /// <returns>The public <see cref="JsonWebKeySet"/> of the server as a <see cref="Task{TResult}"/>.</returns>
        Task<JsonWebKeySet> GetJwks(CancellationToken cancellationToken = default);
    }
}