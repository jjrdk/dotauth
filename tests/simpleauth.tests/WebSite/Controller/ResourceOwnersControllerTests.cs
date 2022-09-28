﻿namespace SimpleAuth.Tests.WebSite.Controller;

using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Repositories;
using Services;
using Shared;
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
                    new Mock<IHttpClientFactory>().Object,
                    inMemoryScopeRepository,
                    new Mock<ILogger<InMemoryClientRepository>>().Object),
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

            var response = await _controller.DeleteMe(CancellationToken.None).ConfigureAwait(false);

            Assert.IsType<BadRequestResult>(response);
        }
    }
}