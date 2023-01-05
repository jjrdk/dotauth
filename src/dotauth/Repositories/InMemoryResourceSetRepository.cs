namespace DotAuth.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Policies;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;

/// <summary>
/// Defines the in-memory resource set repository.
/// </summary>
/// <seealso cref="IResourceSetRepository" />
internal sealed class InMemoryResourceSetRepository : IResourceSetRepository
{
    private readonly IAuthorizationPolicy _policy;
    private readonly ICollection<OwnedResourceSet> _resources;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryResourceSetRepository"/> class.
    /// </summary>
    /// <param name="policy">The resource policy validator</param>
    /// <param name="resources">The resources.</param>
    public InMemoryResourceSetRepository(IAuthorizationPolicy policy, IEnumerable<(string owner, ResourceSet resource)>? resources = null)
    {
        _policy = policy;
        _resources = resources?.Select(x => new OwnedResourceSet(x.owner, x.resource)).ToList() ?? new List<OwnedResourceSet>();
    }

    /// <inheritdoc />
    public  Task<bool> Remove(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        var policy = _resources.FirstOrDefault(p => p.Resource.Id == id);
        if (policy == null)
        {
            return Task.FromResult(false);
        }

        _resources.Remove(policy);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public  Task<ResourceSet?> Get(string owner, string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        var rec = _resources.FirstOrDefault(p => p.Owner == owner && p.Resource.Id == id);

        return Task.FromResult(rec?.Resource);
    }

    /// <inheritdoc />
    public  Task<ResourceSet[]> Get(CancellationToken cancellationToken = default, params string[] ids)
    {
        if (ids == null)
        {
            throw new ArgumentNullException(nameof(ids));
        }

        var result = _resources.Where(r => ids.Contains(r.Resource.Id)).Select(x => x.Resource).ToArray();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public  Task<string?> GetOwner(CancellationToken cancellationToken = default, params string[] ids)
    {
        var owners = _resources.Where(r => ids.Contains(r.Resource.Id)).Select(x => x.Owner).Distinct();

        return Task.FromResult(owners.SingleOrDefault());
    }

    /// <inheritdoc />
    public  Task<ResourceSet[]> GetAll(string owner, CancellationToken cancellationToken = default)
    {
        var result = _resources.Where(x => x.Owner == owner).Select(x => x.Resource).ToArray();
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public  Task<bool> Add(string owner, ResourceSet resourceSet, CancellationToken cancellationToken = default)
    {
        if (resourceSet == null)
        {
            throw new ArgumentNullException(nameof(resourceSet));
        }

        _resources.Add(new OwnedResourceSet(owner, resourceSet));
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public  async Task<PagedResult<ResourceSet>> Search(
        ClaimsPrincipal owner,
        SearchResourceSet? parameter,
        CancellationToken cancellationToken = default)
    {
        if (parameter?.Terms.Length == 0)
        {
            throw new ArgumentNullException(nameof(parameter));
        }

        var result = _resources.AsEnumerable();//.Select(x => x.Resource);

            result = result.Where(
                r => parameter!.Terms.Any(t => r.Resource.Name.Contains(t, StringComparison.OrdinalIgnoreCase))
                     || parameter.Terms.Any(t => r.Resource.Description.Contains(t, StringComparison.OrdinalIgnoreCase))
                     || parameter.Terms.Any(t => r.Resource.Type.Contains(t, StringComparison.OrdinalIgnoreCase)));
        
        if (parameter.Types.Any())
        {
            result = result.Where(r => parameter.Types.Contains(r.Resource.Type, StringComparer.OrdinalIgnoreCase));
        }

        var asyncResult = await Filter(owner, result, cancellationToken);

        IEnumerable<ResourceSet> sortedResult = asyncResult.OrderBy(c => c.Name);
        var nbResult = asyncResult.Length;
        if (parameter.TotalResults > 0)
        {
            sortedResult = sortedResult.Skip(parameter.StartIndex).Take(parameter.TotalResults);
        }

        return new PagedResult<ResourceSet>
        {
            Content = sortedResult.ToArray(),
            StartIndex = parameter.StartIndex,
            TotalResults = nbResult
        };
    }
    
    private async Task<ResourceSet[]> Filter(ClaimsPrincipal requestor, IEnumerable<InMemoryResourceSetRepository.OwnedResourceSet> resourceSets, CancellationToken cancellationToken)
    {
        List<ResourceSet> results = new();
        foreach (var resourceSet in resourceSets)
        {
            var ticket = new TicketLineParameter(requestor.GetClientId(), new[] { "search" });
            if ((await _policy.Execute(
                    ticket,
                    UmaConstants.IdTokenType,
                    requestor,
                    cancellationToken,
                    resourceSet.Resource.AuthorizationPolicies)).Result
                == AuthorizationPolicyResultKind.Authorized)
            {
                results.Add(resourceSet.Resource);
            }
        }

        return results.ToArray();
    }

    /// <inheritdoc />
    public  Task<Option> Update(ResourceSet resourceSet, CancellationToken cancellationToken = default)
    {
        if (resourceSet == null)
        {
            throw new ArgumentNullException(nameof(resourceSet));
        }

        var rec = _resources.FirstOrDefault(p => p.Resource.Id == resourceSet.Id);
        if (rec == null)
        {
            return Task.FromResult<Option>(new Option.Error(new ErrorDetails
            {
                Status = HttpStatusCode.NotFound,
                Title = ErrorCodes.NotUpdated,
                Detail = SharedStrings.ResourceCannotBeUpdated
            }));
        }

        _resources.Remove(rec);
        var res = rec.Resource with
        {
            AuthorizationPolicies = resourceSet.AuthorizationPolicies,
            IconUri = resourceSet.IconUri,
            Name = resourceSet.Name,
            Scopes = resourceSet.Scopes,
            Type = resourceSet.Type
        };
        rec = new OwnedResourceSet(rec.Owner, res);
        _resources.Add(rec);
        return Task.FromResult<Option>(new Option.Success());
    }

    private sealed class OwnedResourceSet
    {
        public OwnedResourceSet(string owner, ResourceSet resource)
        {
            Owner = owner;
            Resource = resource;
        }

        public string Owner { get; }
        public ResourceSet Resource { get; }
    }
}