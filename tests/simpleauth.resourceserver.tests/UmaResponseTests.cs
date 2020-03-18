namespace SimpleAuth.ResourceServer.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using Moq;
    using SimpleAuth.Client;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;
    using Xunit;

    public class UmaResponseTests
    {
        [Fact]
        public async Task WhenReturningUmaResponseForSingleResourceThenGeneratesTicket()
        {
            var permissionClient = new Mock<IUmaPermissionClient>();
            permissionClient
                .Setup(
                    x => x.RequestPermission(
                        It.IsAny<string>(),
                        It.IsAny<PermissionRequest>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new GenericResponse<PermissionResponse> { Content = new PermissionResponse { TicketId = "123" } });
            var response = new UmaResponse(
                new Uri("http://localhost"),
                permissionClient.Object,
                null,
                new PermissionRequest { ResourceSetId = "abc", Scopes = new[] { "read" } });
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer testtoken";
            var context = new ActionContext { HttpContext = httpContext };

            await response.ExecuteResultAsync(context).ConfigureAwait(false);

            var responseHeader = httpContext.Response.Headers[HeaderNames.WWWAuthenticate];
            Assert.NotNull(responseHeader.ToString());
        }

        [Fact]
        public async Task WhenReturningUmaResponseForMultipleResourcesThenGeneratesTicket()
        {
            var permissionClient = new Mock<IUmaPermissionClient>();
            permissionClient
                .Setup(
                    x => x.RequestPermissions(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<PermissionRequest[]>()))
                .ReturnsAsync(
                    new GenericResponse<PermissionResponse> { Content = new PermissionResponse { TicketId = "123" } });
            var response = new UmaResponse(
                new Uri("http://localhost"),
                permissionClient.Object,
                null,
                new PermissionRequest { ResourceSetId = "abc", Scopes = new[] { "read" } },
                new PermissionRequest { ResourceSetId = "def", Scopes = new[] { "read" } });
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer testtoken";
            var context = new ActionContext { HttpContext = httpContext };

            await response.ExecuteResultAsync(context).ConfigureAwait(false);

            var responseHeader = httpContext.Response.Headers[HeaderNames.WWWAuthenticate];
            Assert.NotNull(responseHeader.ToString());
        }
    }
}
