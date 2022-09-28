namespace SimpleAuth.Shared.Repositories;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the JSON Web Key Set read-only store interface
/// </summary>
public interface IJwksStore
{
    /// <summary>
    /// Gets all available public keys as a <see cref="JsonWebKeySet"/>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<JsonWebKeySet> GetPublicKeys(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the signing key.
    /// </summary>
    /// <param name="alg"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<SigningCredentials> GetSigningKey(string alg, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the signing key.
    /// </summary>
    /// <param name="alg"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<SecurityKey> GetEncryptionKey(string alg, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default signing key
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>The default <see cref="SigningCredentials"/>.</returns>
    Task<SigningCredentials> GetDefaultSigningKey(CancellationToken cancellationToken = default);
}