namespace DotAuth.Uma;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class StaticResourceMap : IResourceMap, IResourceMapper
{
    private readonly Dictionary<string, string> _map;
    private readonly Dictionary<string, string> _reverseMap;

    public StaticResourceMap(IReadOnlySet<KeyValuePair<string, string>> mappings)
    {
        _map = mappings.ToDictionary(x => x.Key, x => x.Value);
        _reverseMap = mappings.ToDictionary(x => x.Value, x => x.Key);
    }

    public Task<string?> GetResourceSetId(string resourceId, CancellationToken cancellationToken = default)
    {
        var result = _map.TryGetValue(resourceId, out var resourceSetId) ? resourceSetId : null;
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<string?> GetResourceId(string resourceSetId, CancellationToken cancellationToken = default)
    {
        var result = _reverseMap.TryGetValue(resourceSetId, out var resourceId) ? resourceId : null;
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task MapResource(string resourceId, string resourceSetId, CancellationToken cancellationToken = default)
    {
        lock (_map)
        {
            _map[resourceId] = resourceSetId;
            _reverseMap[resourceSetId] = resourceId;
        }

        return Task.CompletedTask;
    }
}
