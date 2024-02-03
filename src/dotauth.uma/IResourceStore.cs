namespace DotAuth.Uma;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the public resource store interface.
/// </summary>
public interface IResourceStore
{
    /// <summary>
    /// Gets the requested <see cref="ResourceRegistration"/>.
    /// </summary>
    /// <param name="resourceId">The registration id.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<ResourceRegistration?> GetById(string resourceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks whether a particular resource is registered for the given owner.
    /// </summary>
    /// <param name="resourceId">The resource to check.</param>
    /// <param name="owner">The owner of the resource.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<bool> Exists(string resourceId, string owner, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers the passed resource as a protected resource.
    /// </summary>
    /// <param name="registration"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<RegistrationData> Register(ResourceRegistration registration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the UMA resource set id for the registration.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="resourceSetId"></param>
    /// <param name="accessPolicyUri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SetResourceSetId(
        string id,
        string resourceSetId,
        Uri accessPolicyUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a slim list of all the user's resources.
    /// </summary>
    /// <param name="subject">The user id.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<RegistrationData[]> GetAll(string subject, CancellationToken cancellationToken = default);
}
