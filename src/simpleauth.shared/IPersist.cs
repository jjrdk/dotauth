namespace SimpleIdentityServer.Shared
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPersist<in T>
    {
        Task<bool> Persist(T item, CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> Delete<TKey>(TKey key, CancellationToken cancellationToken = default(CancellationToken));
    }
}