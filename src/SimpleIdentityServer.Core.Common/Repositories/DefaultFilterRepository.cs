using SimpleIdentityServer.AccountFilter.Basic.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic.Repositories
{
    public sealed class DefaultFilterRepository : IFilterRepository
    {
        private readonly List<FilterAggregate> _filters;

        public DefaultFilterRepository(List<FilterAggregate> filters)
        {
            _filters = filters ?? new List<FilterAggregate>();
        }

        public Task<string> Add(FilterAggregate filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            filter.Id = Guid.NewGuid().ToString();
            filter.CreateDateTime = filter.CreateDateTime;
            _filters.Add(new FilterAggregate
            {
                CreateDateTime = DateTime.UtcNow,
                Id = filter.Id,
                Name = filter.Name,
                UpdateDateTime = DateTime.UtcNow,
                Rules = filter.Rules.Select(r => new FilterAggregateRule
                {
                    ClaimKey = r.ClaimKey,
                    ClaimValue = r.ClaimValue,
                    Id = r.Id,
                    Operation = r.Operation
                }).ToArray()
            });
            return Task.FromResult(filter.Id);
        }

        public Task<bool> Delete(string filterId)
        {
            if (string.IsNullOrWhiteSpace(filterId))
            {
                throw new ArgumentNullException(nameof(filterId));
            }

            var filter = _filters.FirstOrDefault(f => f.Id == filterId);
            if (filter == null)
            {
                return Task.FromResult(false);
            }

            _filters.Remove(filter);
            return Task.FromResult(true);
        }

        public Task<FilterAggregate> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var filter = _filters.FirstOrDefault(f => f.Id == id);
            if (filter == null)
            {
                return Task.FromResult((FilterAggregate)null);
            }

            return Task.FromResult(filter);
        }

        public Task<IEnumerable<FilterAggregate>> GetAll()
        {
            return Task.FromResult(_filters.Select(f => f));
        }

        public Task<bool> Update(FilterAggregate filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var record = _filters.FirstOrDefault(f => f.Id == filter.Id);
            if (record == null)
            {
                return Task.FromResult(false);
            }
            record.Name = filter.Name;
            record.Rules = filter.Rules;
            record.CreateDateTime = filter.CreateDateTime;
            record.UpdateDateTime = DateTime.UtcNow;
            return Task.FromResult(true);
        }
    }
}
