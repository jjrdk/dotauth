namespace SimpleAuth.Shared
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the event publisher interface.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes the specified evt.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt">The evt.</param>
        /// <returns></returns>
        Task Publish<T>(T evt) where T : Event;
    }
}
