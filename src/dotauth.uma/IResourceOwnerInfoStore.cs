namespace DotAuth.Uma;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the resource owner store interface
/// </summary>
public interface IResourceOwnerInfoStore
{
    /// <summary>
    /// Registers the user with tokens.
    /// </summary>
    /// <param name="resourceOwnerInfo">The user registration.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<bool> Set(ResourceOwnerInfo resourceOwnerInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the registered user with refreshed tokens.
    /// </summary>
    /// <param name="subject">The user subject.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<ResourceOwnerInfo?> Get(string subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the <see cref="ResourceOwnerInfo"/> for the subject.
    /// </summary>
    /// <param name="subject">The subject to remove.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns><c>true</c> if key is deleted, otherwise <c>false</c>.</returns>
    Task<bool> Remove(string subject, CancellationToken cancellationToken = default);
}