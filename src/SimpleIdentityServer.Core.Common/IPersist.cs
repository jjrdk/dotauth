namespace SimpleIdentityServer.Core.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPersist<in T>
    {
        Task<bool> Persist(T item, CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> Delete<TKey>(TKey key, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IProvide<T>
    {
        Task<T> Get(string id, CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<T>> Get(Expression<Func<T, bool>> query,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IStore<T> : IPersist<T>, IProvide<T> { }
}