namespace SimpleAuth
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <inheritdoc />
    public class NoopThrottle : IRequestThrottle
    {
        private NoopThrottle() { }

        /// <summary>
        /// Returns the default instance of the <see cref="NoopThrottle"/>.
        /// </summary>
        public static IRequestThrottle Default { get; } = new NoopThrottle();

        /// <inheritdoc />
        public Task<bool> Allow(HttpRequest request)
        {
            return Task.FromResult(true);
        }
    }
}