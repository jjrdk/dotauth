namespace SimpleIdentityServer.Scim.Host.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Shared;
    using Shared.DTOs;

    internal class InMemoryGroupsRepository : IStore<GroupResource>
    {
        private readonly List<GroupResource> _groups = new List<GroupResource>();

        public Task<bool> Persist(GroupResource item, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (item == null || _groups.Exists(g => g.Id == item.Id))
            {
                return Task.FromResult(false);
            }

            _groups.Add(item);

            return Task.FromResult(true);
        }

        public Task<bool> Delete<TKey>(TKey key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = _groups.FindAll(x => string.Equals(x.Id, key));
            if (result.Count == 1)
            {
                var removed = _groups.Remove(result[0]);
                return Task.FromResult(removed);
            }

            return Task.FromResult(false);
        }

        public Task<GroupResource> Get(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var group = _groups.Find(x => x.Id == id);
            return Task.FromResult(group);
        }

        public Task<IEnumerable<GroupResource>> Get(Expression<Func<GroupResource, bool>> query, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}