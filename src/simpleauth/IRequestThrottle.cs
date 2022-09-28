namespace SimpleAuth;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Defines the rate limiter interface.
/// </summary>
public interface IRequestThrottle
{
    /// <summary>
    /// Checks whether the <see cref="HttpRequest"/> should be processed.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest"/>.</param>
    /// <returns><c>true</c> if request is within rate limit, otherwise <c>false</c>.</returns>
    Task<bool> Allow(HttpRequest request);
}