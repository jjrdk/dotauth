namespace DotAuth.Uma;

using System.Threading;
using System.Threading.Tasks;

public interface IResourceMap
{
    Task<string?> GetResourceSetId(string resourceId, CancellationToken cancellationToken = default);
    Task<string?> GetResourceId(string resourceSetId, CancellationToken cancellationToken = default);
}

public interface IResourceMapper
{
    Task MapResource(string resourceId, string resourceSetId, CancellationToken cancellationToken = default);
}