namespace SimpleAuth.Repositories
{
    using Shared.Models;
    using SimpleAuth.Shared.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Repositories;
    using Models = SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the in-memory resource set repository.
    /// </summary>
    /// <seealso cref="IResourceSetRepository" />
    public sealed class InMemoryResourceSetRepository : IResourceSetRepository
    {
        private readonly ICollection<Models.ResourceSetModel> _resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryResourceSetRepository"/> class.
        /// </summary>
        /// <param name="resources">The resources.</param>
        public InMemoryResourceSetRepository(IReadOnlyCollection<Models.ResourceSetModel> resources = null)
        {
            _resources = resources?.ToList() ?? new List<Models.ResourceSetModel>();
        }

        /// <inheritdoc />
        public Task<bool> Remove(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var policy = _resources.FirstOrDefault(p => p.Id == id);
            if (policy == null)
            {
                return Task.FromResult(false);
            }

            _resources.Remove(policy);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<Models.ResourceSetModel> Get(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var rec = _resources.FirstOrDefault(p => p.Id == id);
            if (rec == null)
            {
                return Task.FromResult((Models.ResourceSetModel) null);
            }

            return Task.FromResult(rec);
        }

        /// <inheritdoc />
        public Task<Models.ResourceSetModel[]> Get(CancellationToken cancellationToken, params string[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var result = _resources.Where(r => ids.Contains(r.Id)).ToArray();
            return Task.FromResult(result);
        }

        /// <param name="cancellationToken"></param>
        /// <inheritdoc />
        public Task<Models.ResourceSetModel[]> GetAll(CancellationToken cancellationToken)
        {
            var result = _resources.ToArray();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> Add(Models.ResourceSetModel resourceSet, CancellationToken cancellationToken)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            _resources.Add(resourceSet);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<GenericResult<Models.ResourceSetModel>> Search(
            SearchResourceSet parameter,
            CancellationToken cancellationToken)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<Models.ResourceSetModel> result = _resources;
            if (parameter.Ids != null && parameter.Ids.Any())
            {
                result = result.Where(r => parameter.Ids.Contains(r.Id));
            }

            if (parameter.Names != null && parameter.Names.Any())
            {
                result = result.Where(r => parameter.Names.Any(n => r.Name.Contains(n)));
            }

            if (parameter.Types != null && parameter.Types.Any())
            {
                result = result.Where(r => parameter.Types.Any(t => r.Type.Contains(t)));
            }

            result = result.OrderBy(c => c.Id);
            var nbResult = result.Count();
            if (parameter.TotalResults > 0)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.TotalResults);
            }

            return Task.FromResult(
                new GenericResult<Models.ResourceSetModel>
                {
                    Content = result.ToArray(), StartIndex = parameter.StartIndex, TotalResults = nbResult
                });
        }

        /// <inheritdoc />
        public Task<bool> Update(Models.ResourceSetModel resourceSet, CancellationToken cancellationToken)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            var rec = _resources.FirstOrDefault(p => p.Id == resourceSet.Id);
            if (rec == null)
            {
                return Task.FromResult(false);
            }

            rec.AuthorizationPolicyIds = resourceSet.AuthorizationPolicyIds;
            rec.IconUri = resourceSet.IconUri;
            rec.Name = resourceSet.Name;
            rec.Policies = resourceSet.Policies;
            rec.Scopes = resourceSet.Scopes;
            rec.Type = resourceSet.Type;
            rec.Uri = resourceSet.Uri;
            return Task.FromResult(true);
        }
    }
}
