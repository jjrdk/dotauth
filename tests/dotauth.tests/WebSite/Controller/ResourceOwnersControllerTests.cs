namespace DotAuth.Tests.WebSite.Controller;

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Endpoints;
using DotAuth.Filters;
using DotAuth.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public static class ResourceOwnersControllerTests
{
    public sealed class GivenAResourceOwnersController
    {
        private readonly InMemoryResourceOwnerRepository _resourceOwnerRepository;
        private readonly InMemoryTokenStore _tokenStore;

        public GivenAResourceOwnersController()
        {
            _resourceOwnerRepository = new InMemoryResourceOwnerRepository(string.Empty);
            _tokenStore = new InMemoryTokenStore();
        }

        [Fact]
        public async Task WhenDeletingSelfAndUserCannotBeDeletedThenReturnsBadRequest()
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "me")]))
            };
            httpContext.RequestServices = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var result = await ResourceOwnersEndpointHandlers.DeleteMe(
                httpContext,
                NoopThrottle.Instance,
                _resourceOwnerRepository,
                _tokenStore,
                new NoOpPublisher(),
                CancellationToken.None);

            await result.ExecuteAsync(httpContext);

            Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        }
    }
}
