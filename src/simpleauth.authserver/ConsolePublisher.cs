namespace SimpleAuth.AuthServer
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    internal class ConsolePublisher : IEventPublisher
    {
        public Task Publish<T>(T evt)
            where T : Event
        {
            Console.WriteLine($"{evt.Id}, {evt.Timestamp}, {evt.GetType().Name}");
            return Task.CompletedTask;
        }
    }
}