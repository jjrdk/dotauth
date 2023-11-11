namespace DotAuth.Tests.WebSite.Controller;

using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Controllers;
using DotAuth.Repositories;
using DotAuth.Services;
using DotAuth.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public static class ResourceOwnersControllerTests
{
    public sealed class GivenAResourceOwnersController
    {
        private readonly ResourceOwnersController _controller;

        public GivenAResourceOwnersController()
        {
            var inMemoryScopeRepository = new InMemoryScopeRepository();
            _controller = new ResourceOwnersController(
                new RuntimeSettings(string.Empty),
                new DefaultSubjectBuilder(),
                new InMemoryResourceOwnerRepository(string.Empty),
                new InMemoryTokenStore(),
                new InMemoryJwksRepository(),
                new InMemoryClientRepository(
                    Substitute.For<IHttpClientFactory>(),
                    inMemoryScopeRepository,
                    Substitute.For<ILogger<InMemoryClientRepository>>()),
                Array.Empty<AccountFilter>(),
                new NoOpPublisher());
        }

        [Fact]
        public async Task WhenDeletingSelfAndUserCannotBeDeletedThenReturnsBadRequest()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "me"), }))
                }
            };

            var response = await _controller.DeleteMe(CancellationToken.None);

            Assert.IsType<BadRequestResult>(response);
        }
    }
}
