namespace DotAuth.AcceptanceTests.Support;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using DotAuth;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using DotAuth.Sms;
using DotAuth.Sms.Ui;
using DotAuth.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using NSubstitute;
using Xunit;

public sealed class ServerStartup
{
    private readonly DotAuthConfiguration _configuration;
    private readonly SharedContext _context;
    private readonly ITestOutputHelper _outputHelper;

    public ServerStartup(SharedContext context, ITestOutputHelper outputHelper)
    {
        IdentityModelEventSource.ShowPII = true;
        var mockConfirmationCodeStore = Substitute.For<IConfirmationCodeStore>();
        mockConfirmationCodeStore.Add(Arg.Any<ConfirmationCode>(), Arg.Any<CancellationToken>())
            .Returns(true);
        mockConfirmationCodeStore.Remove(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        mockConfirmationCodeStore.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                new ConfirmationCode
                {
                    ExpiresIn = TimeSpan.FromDays(10).TotalSeconds,
                    IssueAt = DateTimeOffset.UtcNow,
                    Subject = "phone",
                    Value = "123"
                });
        var symmetricAlgorithm = Aes.Create();
        symmetricAlgorithm.GenerateIV();
        symmetricAlgorithm.GenerateKey();
        _configuration = new DotAuthConfiguration
        {
            DataProtector = _ => new SymmetricDataProtector(symmetricAlgorithm),
            AdministratorRoleDefinition = default,
            JsonWebKeys = _ =>
            {
                var keyset = new[] { context.SignatureKey, context.EncryptionKey }.ToJwks();
                return new InMemoryJwksRepository(keyset, keyset);
            },
            ConfirmationCodes = _ => mockConfirmationCodeStore,
            Clients =
                _ => new InMemoryClientRepository(
                    new TestHttpClientFactory(context.Client!),
                    new InMemoryScopeRepository(),
                    Substitute.For<ILogger<InMemoryClientRepository>>(),
                    DefaultStores.Clients(context)),
            Scopes = _ => new InMemoryScopeRepository(DefaultStores.Scopes()),
            Consents = _ => new InMemoryConsentRepository(DefaultStores.Consents()),
            Users = _ => new InMemoryResourceOwnerRepository(string.Empty, DefaultStores.Users()),
            ClaimsIncludedInUserCreation = ["acceptance_test"],
            DeviceAuthorizationLifetime = TimeSpan.FromSeconds(75),
            DevicePollingInterval = TimeSpan.FromSeconds(3)
        };
        _context = context;
        _outputHelper = outputHelper;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<HttpClient>(_ => { });
//            .AddHttpMessageHandler(_ => new TestDelegatingHandler(_context.Handler!));
        services.AddTransient(_ => _context.Client!);
        var mockSmsClient = Substitute.For<ISmsClient>();
        mockSmsClient.SendMessage(Arg.Any<string>(), Arg.Any<string>()).Returns((true, null));

        services.AddCors(options =>
            options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        services.AddDotAuthServer(
                _configuration,
                [
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    JwtBearerDefaults.AuthenticationScheme
                ])
            .AddDotAuthUi(typeof(IDefaultUi), typeof(IDefaultSmsUi))
            .AddSmsAuthentication(mockSmsClient);
        services
            .AddLogging(l => l.AddXUnit(_outputHelper))
            .AddAccountFilter()
            .AddAuthentication(cfg =>
            {
                cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                _ => { });

        services.AddUmaClient(new Uri("http://localhost/"));
        services.ConfigureOptions<JwtBearerPostConfigureOptions>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseDotAuthServer(applicationTypes: typeof(IDefaultUi));

        // Register test support endpoints (Data endpoint) as minimal API handlers so
        // acceptance tests can exercise UMA-protected resources without relying on MVC
        // controller dispatch. This mirrors the behavior of the original
        // `DataController.Index` action.
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("Data/{id}", async (HttpContext httpContext, string id, UmaClient umaClient, CancellationToken cancellationToken) =>
            {
                var userIdentity = httpContext.User.Identity as ClaimsIdentity;
                var umaTickets = userIdentity!.TryGetUmaTickets(out var permissions);
                if (umaTickets && permissions.Any(x => x.ResourceSetId == id))
                {
                    return Results.Json("Hello");
                }

                var token = await httpContext.GetTokenAsync("access_token");
                var request = new PermissionRequest { ResourceSetId = id, Scopes = ["api1"] };
                var option = await umaClient.RequestPermission(token!, cancellationToken, request);
                if (option is Option<TicketResponse>.Result ticket)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    httpContext.Response.Headers[HeaderNames.WWWAuthenticate] =
                        $"UMA as_uri=\"{umaClient.Authority.AbsoluteUri}\", ticket=\"{ticket.Item.TicketId}\"";

                    return Results.StatusCode((int)HttpStatusCode.Unauthorized);
                }

                return Results.BadRequest(option as Option<TicketResponse>.Error);
            }).RequireAuthorization();
        });
    }
}
