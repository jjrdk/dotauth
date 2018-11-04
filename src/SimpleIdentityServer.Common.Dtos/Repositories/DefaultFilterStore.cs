namespace SimpleIdentityServer.Shared.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AccountFiltering;

    public sealed class DefaultFilterStore : IFilterStore
    {
        private readonly List<Filter> _filters;

        public DefaultFilterStore(List<Filter> filters)
        {
            _filters = filters ?? new List<Filter>();
        }

        public Task<IEnumerable<Filter>> GetAll()
        {
            return Task.FromResult(_filters.Select(f => f));
        }
    }
}
