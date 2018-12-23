namespace SimpleIdentityServer.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProvide<T>
    {
        Task<T> Get(string id, CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<T>> Get(Expression<Func<T, bool>> query,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}