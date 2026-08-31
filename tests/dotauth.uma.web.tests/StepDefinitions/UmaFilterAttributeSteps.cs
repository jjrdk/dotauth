namespace DotAuth.Uma.Web.Tests.StepDefinitions;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Uma.Web.Tests.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Reqnroll;

[Binding]
public class UmaFilterAttributeSteps
{
    private readonly ScenarioContext _ctx;

    // Uses the same mocked services as the handler steps (configured in Background)
    private readonly UmaMockedServices _mocks;

    private UmaFilterAttribute? _filterAttribute;
    private AuthorizationFilterContext? _filterContext;
    private IAuthenticationService? _authService;

    public UmaFilterAttributeSteps(ScenarioContext ctx, UmaMockedServices mocks)
    {
        _ctx = ctx;
        _mocks = mocks;
    }

    // -----------------------------------------------------------------------
    // Given
    // -----------------------------------------------------------------------

    [Given("a filter for resource parameter {string} requiring scope {string}")]
    public void GivenFilter(string paramName, string scope)
    {
        _filterAttribute = new UmaFilterAttribute(paramName, resourceSetAccessScope: scope);
    }

    [Given("a filter for resource parameter {string} requiring scope {string} with allowed scope {string}")]
    public void GivenFilterWithAllowedScope(string paramName, string requiredScope, string allowedScope)
    {
        _filterAttribute = new UmaFilterAttribute(
            paramName,
            allowedOauthScope: allowedScope,
            resourceSetAccessScope: requiredScope);
    }

    [Given("an unauthenticated HTTP context with route value {string} = {string}")]
    public void GivenUnauthenticatedContext(string routeKey, string routeValue) =>
        BuildContext(new ClaimsPrincipal(new ClaimsIdentity()), routeKey, routeValue);

    [Given("an authenticated user with permissions for resource set {string} scope {string}")]
    public void GivenPermissions(string rsId, string scope)
    {
        _ctx["perm_rsid"] = rsId;
        _ctx["perm_scope"] = scope;
    }

    [Given("the HTTP context route value {string} = {string}")]
    public void GivenRouteValue(string routeKey, string routeValue)
    {
        ClaimsIdentity identity;
        if (_ctx.TryGetValue<string>("oauth_scope", out var oauthScope))
        {
            // Scenario includes an OAuth scope that should short-circuit UMA checks.
            identity = new ClaimsIdentity(
                [new Claim("sub", "u"), new Claim(StandardClaimNames.Scopes, oauthScope)], "Bearer");
        }
        else if (_ctx.TryGetValue<string>("perm_rsid", out var rsId)
            && _ctx.TryGetValue<string>("perm_scope", out var scope))
        {
            var perm = new Permission
            {
                ResourceSetId = rsId,
                Scopes = [scope],
                Expiry = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                IssuedAt = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds()
            };
            var json = JsonSerializer.Serialize(perm, SharedSerializerContext.Default.Permission);
            identity = new ClaimsIdentity(
                [new Claim("sub", "u"), new Claim("permissions", json)], "Bearer");
        }
        else
        {
            identity = new ClaimsIdentity([new Claim("sub", "u")], "Bearer");
        }

        BuildContext(new ClaimsPrincipal(identity), routeKey, routeValue);
    }

    [Given("a user with OAuth scope {string}")]
    public void GivenOAuthScope(string oauthScope) => _ctx["oauth_scope"] = oauthScope;

    // -----------------------------------------------------------------------
    // When
    // -----------------------------------------------------------------------

    [When("the filter runs authorization")]
    public async Task WhenFilterRunsAuthorization()
    {
        Assert.NotNull(_filterContext);
        Assert.NotNull(_filterAttribute);

        var sp = _filterContext!.HttpContext.RequestServices;
        var filter = (IAsyncAuthorizationFilter)_filterAttribute!.CreateInstance(sp);
        await filter.OnAuthorizationAsync(_filterContext);
    }

    // -----------------------------------------------------------------------
    // Then
    // -----------------------------------------------------------------------

    [Then("ChallengeAsync is called with the UMA scheme")]
    public async Task ThenChallengeAsyncIsCalled()
    {
        await _authService!.Received(1).ChallengeAsync(
            _filterContext!.HttpContext,
            UmaBearerDefaults.AuthenticationScheme,
            Arg.Any<AuthenticationProperties>());
    }

    [Then("the filter result is null")]
    public void ThenResultIsNull() => Assert.Null(_filterContext!.Result);

    // -----------------------------------------------------------------------
    // Helper
    // -----------------------------------------------------------------------

    private void BuildContext(ClaimsPrincipal user, string routeKey, string routeValue)
    {
        _authService = Substitute.For<IAuthenticationService>();

        var services = new ServiceCollection();
        services.AddSingleton(_mocks.ResourceMap);
        services.AddSingleton<ILogger<UmaFilterAttribute>>(_ => NullLogger<UmaFilterAttribute>.Instance);
        services.AddSingleton(_authService);

        var sp = services.BuildServiceProvider();
        var httpCtx = new DefaultHttpContext { User = user, RequestServices = sp };
        var routeData = new RouteData { Values = { [routeKey] = routeValue } };
        _filterContext = new AuthorizationFilterContext(
            new ActionContext(httpCtx, routeData, new ActionDescriptor()),
            new List<IFilterMetadata>());
    }
}
