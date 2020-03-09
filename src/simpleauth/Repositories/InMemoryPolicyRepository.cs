namespace SimpleAuth.Repositories
{
    using Shared.Models;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the in-memory policy repository.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Repositories.IPolicyRepository" />
    public sealed class InMemoryPolicyRepository : IPolicyRepository
    {
        private readonly ICollection<Policy> _policies;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryPolicyRepository"/> class.
        /// </summary>
        /// <param name="policies">The policies.</param>
        public InMemoryPolicyRepository(IReadOnlyCollection<Policy> policies = null)
        {
            _policies = policies == null ? new List<Policy>() : policies.ToList();
        }

        /// <inheritdoc />
        public Task<bool> Add(Policy policy, CancellationToken cancellationToken = default)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            _policies.Add(policy);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Delete(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var policy = _policies.FirstOrDefault(p => p.Id == id);
            if (policy == null)
            {
                return Task.FromResult(false);
            }

            _policies.Remove(policy);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<Policy> Get(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var r = _policies.FirstOrDefault(p => p.Id == id);
            if (r == null)
            {
                return Task.FromResult((Policy)null);
            }

            return Task.FromResult(r);
        }

        /// <inheritdoc />
        public Task<Policy[]> GetAll(string owner, CancellationToken cancellationToken)
        {
            var result = _policies.Where(p => p.Owner == owner).ToArray();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<GenericResult<Policy>> Search(
            SearchAuthPolicies parameter,
            CancellationToken cancellationToken = default)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<Policy> result = _policies;
            if (parameter.Ids != null && parameter.Ids.Any())
            {
                result = result.Where(r => parameter.Ids.Contains(r.Id));
            }

            var nbResult = result.Count();
            result = result.OrderBy(c => c.Id);
            if (parameter.TotalResults > 0)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.TotalResults);
            }

            return Task.FromResult(new GenericResult<Policy>
            {
                Content = result.ToArray(),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        /// <inheritdoc />
        public Task<bool> Update(Policy policy, CancellationToken cancellationToken = default)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            var rec = _policies.FirstOrDefault(p => p.Id == policy.Id);
            if (rec == null)
            {
                return Task.FromResult(false);
            }

            rec.Rules = policy.Rules;
            return Task.FromResult(true);
        }
    }
}
