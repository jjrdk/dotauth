namespace DotAuth.Uma.Tests;

using Microsoft.AspNetCore.Authentication;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shared.Models;
using Shared.Requests;
using Shared.Responses;
using Web;

public class UmaFilterAttributeTests
{
    private readonly UmaFilterAttribute _filterAttribute = new(
        "resource",
        allowedOauthScope: "tester",
        resourceSetAccessScope: "read");

    private readonly ServiceCollection _serviceCollection;

    public UmaFilterAttributeTests()
    {
        _serviceCollection = [];
        _serviceCollection.AddTransient(_ =>
        {
            var mock = Substitute.For<ITokenClient>();
            mock.GetToken(Arg.Any<TokenRequest>(), Arg.Any<CancellationToken>())
                .Returns(new GrantedTokenResponse { AccessToken = "abc", IdToken = "def" });
            return mock;
        });
        _serviceCollection.AddTransient(_ =>
        {
            var mock = Substitute.For<IUmaPermissionClient>();
            mock.Authority.Returns(new Uri("http://localhost"));
            mock.RequestPermission(
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>(),
                    Arg.Any<PermissionRequest[]>())
                .Returns(new TicketResponse { TicketId = "abc" });
            return mock;
        });
        _serviceCollection.AddSingleton(Substitute.For<IAuthenticationService>());
        _serviceCollection.AddTransient<ILogger<UmaFilterAttribute>>(_ => NullLogger<UmaFilterAttribute>.Instance);
        _serviceCollection.AddSingleton<IResourceMap>(_ =>
            new StaticResourceMap(new HashSet<KeyValuePair<string, string>>([KeyValuePair.Create("a", "a")])));
    }

    [Fact]
    public async Task PermitsUserWithValidScope()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim("scope", "tester")], JwtBearerConstants.BearerScheme))
        };
        var result = await GetFilterResult(httpContext);

        Assert.Null(result);
    }

    [Fact]
    public async Task PermitsUserWithValidPermission()
    {
        var claims = new[]
        {
            new Claim(
                "permissions",
                JsonSerializer.Serialize(
                    new Permission
                    {
                        Expiry = DateTimeOffset.MaxValue.ToUnixTimeSeconds(),
                        IssuedAt = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
                        ResourceSetId = "a",
                        Scopes = ["read"]
                    }, SharedSerializerContext.Default.Permission))
        };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerConstants.BearerScheme))
        };
        var result = await GetFilterResult(httpContext);

        Assert.Null(result);
    }

    [Fact]
    public async Task RequestsTicketWhenNoPermissionsInToken()
    {
        var sp = _serviceCollection.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        var authService = sp.GetRequiredService<IAuthenticationService>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = sp,
            Request = { Headers = { ["id_token"] = "******" } },
            User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim("sub", "tester")], JwtBearerConstants.BearerScheme)),
        };
        var filterResult = await GetFilterResult(httpContext);

        // The filter now delegates ticket issuance to UmaBearerHandler via ChallengeAsync.
        Assert.IsType<EmptyResult>(filterResult);
        await authService.Received(1).ChallengeAsync(
            httpContext,
            UmaBearerDefaults.AuthenticationScheme,
            Arg.Any<AuthenticationProperties>());
    }
    private async Task<IActionResult?> GetFilterResult(DefaultHttpContext httpContext)
    {
        var authFilter =
            _filterAttribute.CreateInstance(_serviceCollection.BuildServiceProvider()) as IAsyncAuthorizationFilter;

        var filterContext = new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData { Values = { ["resource"] = "a" } }, new ActionDescriptor()),
            new List<IFilterMetadata>());
        await authFilter!.OnAuthorizationAsync(filterContext).ConfigureAwait(false);
        return filterContext.Result;
    }
}

