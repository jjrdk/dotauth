namespace SimpleIdentityServer.Shared
{
    public interface IStore<T> : IPersist<T>, IProvide<T> { }
}