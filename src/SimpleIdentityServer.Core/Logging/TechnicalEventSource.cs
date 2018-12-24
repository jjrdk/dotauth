namespace SimpleIdentityServer.Core.Logging
{
    using Microsoft.Extensions.Logging;

    public class TechnicalEventSource : BaseEventSource, ITechnicalEventSource
    {
        public TechnicalEventSource(ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger<TechnicalEventSource>())
        {
        }
    }
}
