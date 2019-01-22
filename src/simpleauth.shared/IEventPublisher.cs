namespace SimpleAuth.Shared
{
    using System.Threading.Tasks;

    public interface IEventPublisher
    {
        Task Publish<T>(T evt) where T : Event;
    }
}
