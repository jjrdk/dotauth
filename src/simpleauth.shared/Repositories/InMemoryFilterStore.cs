namespace SimpleAuth.Shared.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the in-memory filter store.
    /// </summary>
    public sealed class InMemoryFilterStore : IFilterStore
    {
        private readonly Filter[] _filters;

        public InMemoryFilterStore(params Filter[] filters)
        {
            _filters = filters ?? Array.Empty<Filter>();
        }

        public Task<Filter[]> GetAll(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_filters.ToArray());
        }
    }
}
