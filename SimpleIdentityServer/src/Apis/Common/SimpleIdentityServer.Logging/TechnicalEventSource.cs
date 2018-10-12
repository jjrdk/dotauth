using Microsoft.Extensions.Logging;

namespace SimpleIdentityServer.Logging
{
    public interface ITechnicalEventSource : IEventSource
    {

    }

    public class TechnicalEventSource : BaseEventSource, ITechnicalEventSource
    {
        public TechnicalEventSource(ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger<TechnicalEventSource>())
        {
        }
    }
}
