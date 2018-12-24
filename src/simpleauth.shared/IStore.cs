namespace SimpleAuth.Shared
{
    public interface IStore<T> : IPersist<T>, IProvide<T> { }
}