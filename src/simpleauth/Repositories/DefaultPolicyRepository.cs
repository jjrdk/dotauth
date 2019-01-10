namespace SimpleAuth.Repositories
{
    using Parameters;
    using Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class DefaultPolicyRepository : IPolicyRepository
    {
        public ICollection<Policy> _policies;

        public DefaultPolicyRepository(IReadOnlyCollection<Policy> policies = null)
        {
            _policies = policies == null ? new List<Policy>() : policies.ToList();
        }

        public Task<bool> Add(Policy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            _policies.Add(policy);
            return Task.FromResult(true);
        }

        public Task<bool> Delete(string id)
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

        public Task<Policy> Get(string id)
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

        public Task<ICollection<Policy>> GetAll()
        {
            ICollection<Policy> result = _policies.ToList();
            return Task.FromResult(result);
        }

        public Task<SearchAuthPoliciesResult> Search(SearchAuthPoliciesParameter parameter)
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
            if (parameter.IsPagingEnabled)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return Task.FromResult(new SearchAuthPoliciesResult
            {
                Content = result,
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<ICollection<Policy>> SearchByResourceId(string resourceSetId)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            ICollection<Policy> result = _policies.Where(p => p.ResourceSetIds.Contains(resourceSetId))
                .ToList();
            return Task.FromResult(result);
        }

        public Task<bool> Update(Policy policy)
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
