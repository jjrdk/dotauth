namespace DotAuth.Uma.Web.Tests.StepDefinitions;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using DotAuth.Uma.Web.Tests.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Reqnroll;

[Binding]
public class UmaBearerHandlerSteps : IAsyncDisposable
{
    private readonly ScenarioContext _scenarioCtx;
    private readonly UmaMockedServices _mocks;

    private IHost? _host;
    private HttpClient? _client;
    private HttpResponseMessage? _lastResponse;

    private string? _realm;

    private readonly SecurityKey _signingKey;
    private readonly string _issuer = "https://issuer.test";

    public UmaBearerHandlerSteps(ScenarioContext scenarioCtx, UmaMockedServices mocks)
    {
        _scenarioCtx = scenarioCtx;
        _mocks = mocks;
        _signingKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes("test-signing-key-32-bytes-long!!!!"));
    }

    // -----------------------------------------------------------------------
    // Background — shared by Challenge, Forbidden, and Authenticate features
    // -----------------------------------------------------------------------

    [Given("the permission client returns ticket {string} and AS URI {string}")]
    public void GivenPermissionClientReturnsTicket(string ticketId, string asUri)
    {
        _mocks.PermissionClient.Authority.Returns(new Uri(asUri));
        _mocks.PermissionClient.RequestPermission(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<PermissionRequest[]>())
            .Returns(new Option<TicketResponse>.Result(new TicketResponse { TicketId = ticketId }));
        _mocks.PermissionClient.GetResourceSetScopes(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(["read"]);
    }

    [Given("the resource map resolves {string} to resource set {string}")]
    public void GivenResourceMapResolves(string resourceId, string resourceSetId)
    {
        _mocks.ResourceMap.GetResourceSetId(resourceId, Arg.Any<CancellationToken>())
            .Returns(resourceSetId);
    }

    [Given("the token client returns a valid protection token")]
    public void GivenTokenClientReturnsProtectionToken()
    {
        _mocks.TokenClient.GetToken(Arg.Any<TokenRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Option<GrantedTokenResponse>.Result(
                new GrantedTokenResponse { AccessToken = "pat-token", TokenType = "Bearer" }));
    }

    // -----------------------------------------------------------------------
    // Server setup
    // -----------------------------------------------------------------------

    [Given("a protected endpoint for resource {string} requiring scope {string}")]
    public Task GivenProtectedEndpoint(string resourceId, string scope) =>
        BuildServerAsync(resourceId, [scope]);

    [Given("the scheme options have Realm set to {string}")]
    public async Task GivenRealm(string realm)
    {
        _realm = realm;
        if (_host is not null) { await DisposeAsync(); }
        // Server will be rebuilt when the next request step runs.
    }

    // -----------------------------------------------------------------------
    // Failure overrides
    // -----------------------------------------------------------------------

    [Given("the token client cannot return a protection token")]
    public void GivenTokenClientFails()
    {
        _mocks.TokenClient.GetToken(Arg.Any<TokenRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Option<GrantedTokenResponse>.Error(
                new DotAuth.Shared.Models.ErrorDetails
                {
                    Title = "Error",
                    Detail = "unavailable",
                    Status = System.Net.HttpStatusCode.InternalServerError
                }));
    }

    [Given("the permission client throws an exception")]
    public void GivenPermissionClientThrows()
    {
        _mocks.PermissionClient.RequestPermission(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<PermissionRequest[]>())
            .Throws(new InvalidOperationException("Permission endpoint unavailable"));
    }

    // -----------------------------------------------------------------------
    // Request actions
    // -----------------------------------------------------------------------

    [When("a request is made without an Authorization header")]
    public async Task WhenRequestWithoutAuthHeader()
    {
        await EnsureServerAsync("resource-a", ["read"]);
        _lastResponse = await _client!.GetAsync("/protected");
    }

    [When("ForbidAsync is called on the scheme")]
    public async Task WhenForbidAsync()
    {
        await EnsureServerAsync("resource-a", ["read"]);
        _lastResponse = await _client!.GetAsync("/forbid");
    }

    // -----------------------------------------------------------------------
    // Authentication feature steps
    // -----------------------------------------------------------------------

    [Given("a valid RPT JWT containing a permissions claim for resource set {string} scope {string}")]
    public void GivenRptWithPermissions(string resourceSetId, string scope)
    {
        var perm = new Permission
        {
            ResourceSetId = resourceSetId,
            Scopes = [scope],
            Expiry = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            IssuedAt = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds()
        };
        var json = JsonSerializer.Serialize(new[] { perm }, SharedSerializerContext.Default.PermissionArray);
        _scenarioCtx["token"] = CreateJwt([new Claim("sub", "u"), new Claim("permissions", json)],
            DateTimeOffset.UtcNow.AddHours(1));
    }

    [Given("a valid JWT that contains no permissions claim")]
    public void GivenJwtWithoutPermissions()
    {
        _scenarioCtx["token"] = CreateJwt([new Claim("sub", "u")], DateTimeOffset.UtcNow.AddHours(1));
    }

    [Given("an RPT JWT with an expiry in the past")]
    public void GivenExpiredJwt()
    {
        _scenarioCtx["token"] = CreateJwt([new Claim("sub", "u")], DateTimeOffset.UtcNow.AddMinutes(-90), notBefore: DateTimeOffset.UtcNow.AddHours(-3));
    }

    [When("HandleAuthenticateAsync is called")]
    public async Task WhenHandleAuthenticateAsync()
    {
        await EnsureAuthServerAsync();
        var token = (string)_scenarioCtx["token"];
        var req = new HttpRequestMessage(HttpMethod.Get, "/authenticate");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _lastResponse = await _client!.SendAsync(req);
    }

    [When("HandleAuthenticateAsync is called against a resource-aware endpoint")]
    public async Task WhenHandleAuthenticateAsyncResourceAware()
    {
        // Build a server with ResourceIdParameters configured so the access check fires.
        await EnsureResourceAwareAuthServerAsync();
        var token = (string)_scenarioCtx["token"];
        var req = new HttpRequestMessage(HttpMethod.Get, "/authenticate/resource-a");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _lastResponse = await _client!.SendAsync(req);
    }

    // -----------------------------------------------------------------------
    // Assertions
    // -----------------------------------------------------------------------

    [Then("the response status is (\\d+)")]
    public void ThenStatusIs(int code) =>
        Assert.Equal(code, (int)_lastResponse!.StatusCode);

    [Then("the WWW-Authenticate header starts with {string}")]
    public void ThenWwwAuthStartsWith(string prefix)
    {
        var header = GetWwwAuth();
        Assert.True(header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase),
            $"Expected '{prefix}' prefix, got: {header}");
    }

    [Then(@"the WWW-Authenticate header contains as_uri=""([^""]+)""")]
    public void ThenWwwAuthContainsAsUri(string uri) =>
        Assert.Contains($"as_uri=\"{uri}\"", GetWwwAuth());

    [Then(@"the WWW-Authenticate header contains ticket=""([^""]+)""")]
    public void ThenWwwAuthContainsTicket(string ticket) =>
        Assert.Contains($"ticket=\"{ticket}\"", GetWwwAuth());

    [Then(@"the WWW-Authenticate header contains realm=""([^""]+)""")]
    public void ThenWwwAuthContainsRealm(string realm) =>
        Assert.Contains($"realm=\"{realm}\"", GetWwwAuth());

    [Then("the result is AuthenticateResult.Success")]
    public void ThenAuthSuccess() =>
        Assert.Equal(System.Net.HttpStatusCode.OK, _lastResponse!.StatusCode);

    [Then("the result is AuthenticateResult.Fail")]
    public void ThenAuthFail() =>
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, _lastResponse!.StatusCode);

    [Then("the ClaimsPrincipal has a {string} claim")]
    public async Task ThenPrincipalHasClaim(string claimType)
    {
        var body = await _lastResponse!.Content.ReadAsStringAsync();
        Assert.Contains($"claim:{claimType}", body, StringComparison.OrdinalIgnoreCase);
    }

    [Then("CheckResourceAccess for {string} with scope {string} returns true")]
    public async Task ThenCheckAccessTrue(string rsId, string scope)
    {
        var body = await _lastResponse!.Content.ReadAsStringAsync();
        Assert.Contains($"access:{rsId}:{scope}:true", body, StringComparison.OrdinalIgnoreCase);
    }

    [Then("CheckResourceAccess returns false for resource set {string}")]
    public async Task ThenCheckAccessFalse(string rsId)
    {
        var body = await _lastResponse!.Content.ReadAsStringAsync();
        Assert.Contains($"access:{rsId}:", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain($"access:{rsId}:read:true", body, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private string GetWwwAuth()
    {
        Assert.True(_lastResponse!.Headers.TryGetValues("WWW-Authenticate", out var vals),
            "Missing WWW-Authenticate header");
        return string.Join(",", vals!);
    }

    private string CreateJwt(IEnumerable<Claim> claims, DateTimeOffset expiry, DateTimeOffset? notBefore = null)
    {
        var nb = (notBefore ?? DateTimeOffset.UtcNow.AddMinutes(-5)).UtcDateTime;
        var handler = new JwtSecurityTokenHandler();
        var desc = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiry.UtcDateTime,
            NotBefore = nb,
            IssuedAt = nb,
            Issuer = _issuer,
            Audience = "test",
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        };
        return handler.CreateEncodedJwt(desc);
    }

    private async Task EnsureServerAsync(string resourceId, string[] scopes)
    {
        if (_host is null) await BuildServerAsync(resourceId, scopes);
    }

    private async Task EnsureAuthServerAsync()
    {
        if (_host is null) await BuildAuthServerAsync();
    }

    private async Task EnsureResourceAwareAuthServerAsync()
    {
        if (_host is null) await BuildResourceAwareAuthServerAsync();
    }

    private Task BuildServerAsync(string resourceId, string[] scopes)
    {
        var realm = _realm;
        var signingKey = _signingKey;
        var issuer = _issuer;

        _host = new HostBuilder().ConfigureWebHost(webHost =>
        {
            webHost.UseTestServer();
            webHost.ConfigureServices(services =>
            {
                services.AddSingleton(_mocks.ResourceMap);
                services.AddSingleton(_mocks.PermissionClient);
                services.AddSingleton(_mocks.TokenClient);
                services.AddAuthentication(UmaBearerDefaults.AuthenticationScheme)
                    .AddUmaBearer(opts =>
                    {
                        opts.Realm = realm;
                        opts.ResourceIdParameters = ["rid"];
                        opts.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true, ValidIssuer = issuer,
                            ValidateAudience = false, IssuerSigningKey = signingKey
                        };
                    });
                services.AddRouting();
            });
            webHost.Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseEndpoints(ep =>
                {
                    ep.MapGet("/protected", async ctx =>
                    {
                        await ctx.ChallengeAsync(UmaBearerDefaults.AuthenticationScheme,
                            new AuthenticationProperties
                            {
                                Items =
                                {
                                    ["uma:resource_set_id"] = resourceId,
                                    ["uma:scopes"] = string.Join(" ", scopes)
                                }
                            });
                    });
                    ep.MapGet("/forbid", async ctx =>
                    {
                        await ctx.ForbidAsync(UmaBearerDefaults.AuthenticationScheme,
                            new AuthenticationProperties
                            {
                                Items =
                                {
                                    ["uma:resource_set_id"] = resourceId,
                                    ["uma:scopes"] = string.Join(" ", scopes)
                                }
                            });
                    });
                });
            });
        }).Build();

        _host.Start();
        _client = _host.GetTestServer().CreateClient();
        return Task.CompletedTask;
    }

    private Task BuildAuthServerAsync()
    {
        var signingKey = _signingKey;
        var issuer = _issuer;

        _host = new HostBuilder().ConfigureWebHost(webHost =>
        {
            webHost.UseTestServer();
            webHost.ConfigureServices(services =>
            {
                services.AddSingleton(_mocks.ResourceMap);
                services.AddSingleton(_mocks.PermissionClient);
                services.AddSingleton(_mocks.TokenClient);
                services.AddAuthentication(UmaBearerDefaults.AuthenticationScheme)
                    .AddUmaBearer(opts =>
                    {
                        opts.MapInboundClaims = false;
                        opts.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true, ValidIssuer = issuer,
                            ValidateAudience = false, IssuerSigningKey = signingKey
                        };
                    });
                services.AddRouting();
            });
            webHost.Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseEndpoints(ep =>
                {
                    ep.MapGet("/authenticate", async ctx =>
                    {
                        var result = await ctx.AuthenticateAsync(UmaBearerDefaults.AuthenticationScheme);
                        if (!result.Succeeded)
                        {
                            ctx.Response.StatusCode = 401;
                            return;
                        }

                        var principal = result.Principal!;
                        var sb = new System.Text.StringBuilder();
                        foreach (var claim in principal.Claims)
                            sb.AppendLine($"claim:{claim.Type}");

                        foreach (var rs in new[] { "rs-001", "rs-any", "rs-002" })
                        {
                            var r = DotAuth.Uma.ClaimsPrincipalExtensions.CheckResourceAccess(principal, rs, "read");
                            sb.AppendLine($"access:{rs}:read:{r}".ToLower());
                        }

                        ctx.Response.StatusCode = 200;
                        await ctx.Response.WriteAsync(sb.ToString());
                    });
                });
            });
        }).Build();

        _host.Start();
        _client = _host.GetTestServer().CreateClient();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Builds a server where the UMA handler is configured with <c>ResourceIdParameters</c>, so
    /// the access check in <c>HandleAuthenticateAsync</c> actually fires against the resolved resource set.
    /// </summary>
    private Task BuildResourceAwareAuthServerAsync()
    {
        var signingKey = _signingKey;
        var issuer = _issuer;

        _host = new HostBuilder().ConfigureWebHost(webHost =>
        {
            webHost.UseTestServer();
            webHost.ConfigureServices(services =>
            {
                services.AddSingleton(_mocks.ResourceMap);
                services.AddSingleton(_mocks.PermissionClient);
                services.AddSingleton(_mocks.TokenClient);
                services.AddAuthentication(UmaBearerDefaults.AuthenticationScheme)
                    .AddUmaBearer(opts =>
                    {
                        opts.MapInboundClaims = false;
                        opts.ResourceIdParameters = ["rid"]; // enables per-request access check
                        opts.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true, ValidIssuer = issuer,
                            ValidateAudience = false, IssuerSigningKey = signingKey
                        };
                    });
                services.AddRouting();
            });
            webHost.Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseEndpoints(ep =>
                {
                    // Route exposes "rid" — the handler reads this to resolve the resource set ID.
                    ep.MapGet("/authenticate/{rid}", async ctx =>
                    {
                        var result = await ctx.AuthenticateAsync(UmaBearerDefaults.AuthenticationScheme);
                        if (!result.Succeeded)
                        {
                            // Trigger challenge so the handler can issue a ticket.
                            await ctx.ChallengeAsync(UmaBearerDefaults.AuthenticationScheme);
                            return;
                        }

                        ctx.Response.StatusCode = 200;
                        await ctx.Response.WriteAsync("ok");
                    });
                });
            });
        }).Build();

        _host.Start();
        _client = _host.GetTestServer().CreateClient();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
        }
    }
}
