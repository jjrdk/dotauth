namespace SimpleAuth.Shared.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    internal sealed class DefaultFilterStore : IFilterStore
    {
        private readonly List<Filter> _filters;

        public DefaultFilterStore(params Filter[] filters)
        {
            _filters = filters == null ? new List<Filter>() : filters.ToList();
        }

        public Task<Filter[]> GetAll(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_filters.ToArray());
        }
    }
}
