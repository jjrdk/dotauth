namespace DotAuth.Uma.Client;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using Shared.Models;

public interface IResourceClient
{
    Task<Option<ResourceResult>> Get(
        string resourceId,
        ResourceOwnerInfo principal,
        CancellationToken cancellationToken = default,
        params string[] scopes);

    Task<Option<PagedResult<ResourceDescription>>> Search(
        string[] terms,
        string idToken,
        CancellationToken cancellationToken);
}