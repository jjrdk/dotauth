﻿namespace SimpleAuth.Repositories
{
    using Shared.Models;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class InMemoryPolicyRepository : IPolicyRepository
    {
        private readonly ICollection<Policy> _policies;

        public InMemoryPolicyRepository(IReadOnlyCollection<Policy> policies = null)
        {
            _policies = policies == null ? new List<Policy>() : policies.ToList();
        }

        public Task<bool> Add(Policy policy, CancellationToken cancellationToken = default)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            _policies.Add(policy);
            return Task.FromResult(true);
        }

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

        public Task<Policy[]> GetAll(CancellationToken cancellationToken = default)
        {
            var result = _policies.ToArray();
            return Task.FromResult(result);
        }

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

            if (parameter.ResourceIds != null && parameter.ResourceIds.Any())
            {
                result = result.Where(p => p.ResourceSetIds.Any(r => parameter.ResourceIds.Contains(r)));
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

        public Task<Policy[]> SearchByResourceId(string resourceSetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            var result = _policies.Where(p => p.ResourceSetIds.Contains(resourceSetId))
                .ToArray();
            return Task.FromResult(result);
        }

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

            rec.ResourceSetIds = policy.ResourceSetIds;
            rec.Rules = policy.Rules;
            return Task.FromResult(true);
        }
    }
}