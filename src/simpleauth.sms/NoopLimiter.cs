namespace SimpleAuth.Sms
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <inheritdoc />
    public class NoopLimiter : IRateLimiter
    {
        /// <summary>
        /// Returns the default instance of the <see cref="NoopLimiter"/>.
        /// </summary>
        public static IRateLimiter Default { get; } = new NoopLimiter();

        /// <inheritdoc />
        public Task<bool> Allow(HttpRequest request)
        {
            return Task.FromResult(true);
        }
    }
}