namespace DotAuth.Stores.Marten.AcceptanceTests;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using DotAuth;
using DotAuth.Extensions;
using DotAuth.Repositories;
using DotAuth.Stores.Marten;
using DotAuth.UI;
using global::Marten;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

public sealed class ServerStartup
{
    private const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;
    private readonly DotAuthConfiguration _martenConfiguration;
    private readonly SharedContext _context;
    private readonly string _connectionString;
    private readonly ITestOutputHelper _outputHelper;
    private readonly string _schemaName;

    public ServerStartup(SharedContext context, string connectionString, ITestOutputHelper outputHelper)
    {
        _martenConfiguration = new DotAuthConfiguration
        {
            AdministratorRoleDefinition = default,
            Clients = sp => new MartenClientStore(sp.GetRequiredService<Func<IDocumentSession>>(),
                sp.GetRequiredService<ILogger<MartenClientStore>>()),
            JsonWebKeys = _ =>
            {
                var keyset = new[] { context.SignatureKey, context.EncryptionKey }.ToJwks();
                return new InMemoryJwksRepository(keyset, keyset);
            },
            Scopes = sp => new MartenScopeRepository(sp.GetRequiredService<Func<IDocumentSession>>()),
            Consents = sp => new MartenConsentRepository(sp.GetRequiredService<Func<IDocumentSession>>()),
            Users = sp => new MartenResourceOwnerStore(string.Empty, sp.GetRequiredService<Func<IDocumentSession>>()),
            ResourceSets =
                sp => new MartenResourceSetRepository(
                    sp.GetRequiredService<Func<IDocumentSession>>(),
                    sp.GetRequiredService<ILogger<MartenResourceSetRepository>>()),
            DeviceAuthorizationLifetime = TimeSpan.FromSeconds(5),
            DevicePollingInterval = TimeSpan.FromSeconds(3)
        };
        _context = context;
        _connectionString = connectionString;
        _outputHelper = outputHelper;
        var builder = new NpgsqlConnectionStringBuilder { ConnectionString = _connectionString};
        _schemaName = builder.SearchPath ?? "public";
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<HttpClient>(_ => { })
            .AddHttpMessageHandler(() => new TestDelegatingHandler(_context.Handler()));
        services.AddSingleton<IDocumentStore>(
            _ => new DocumentStore(
                new DotAuthMartenOptions(
                    _connectionString,
                    new MartenLoggerFacade(NullLogger<MartenLoggerFacade>.Instance),
                    _schemaName)));
        services.AddTransient<Func<IDocumentSession>>(
            sp =>
            {
                var store = sp.GetRequiredService<IDocumentStore>();
                return () => store.LightweightSession();
            });

        services.AddCors(
            options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        services.AddDotAuthServer(
                _martenConfiguration,
                [DefaultSchema, JwtBearerDefaults.AuthenticationScheme])
            .AddDotAuthUi(typeof(IDefaultUi));
        services.AddLogging(l => l.AddXUnit(_outputHelper)).AddAccountFilter().AddSingleton(_ => _context.Client);
        services.AddAuthentication(
                cfg =>
                {
                    cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    cfg.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                    cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
            .AddCookie(DefaultSchema)
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.TokenValidationParameters = new NoOpTokenValidationParameters(_context);
                });

        services.AddUmaClient(new Uri("http://localhost/"));
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseDotAuthServer(applicationTypes: typeof(IDefaultUi));

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("Data/{id}", async (HttpContext httpContext, string id, UmaClient umaClient, CancellationToken cancellationToken) =>
            {
                var userIdentity = httpContext.User.Identity as ClaimsIdentity;
                if (userIdentity!.TryGetUmaTickets(out var permissions) && permissions.Any(x => x.ResourceSetId == id))
                {
                    return Results.Json("Hello");
                }

                var token = await httpContext.GetTokenAsync("access_token").ConfigureAwait(false);
                var request = new PermissionRequest { ResourceSetId = id, Scopes = new[] { "api1" } };
                var option = await umaClient.RequestPermission(token!, cancellationToken, request).ConfigureAwait(false);
                var ticket = option as Option<TicketResponse>.Result;
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                httpContext.Response.Headers[HeaderNames.WWWAuthenticate] =
                    $"UMA as_uri=\"{umaClient.Authority.AbsoluteUri}\", ticket=\"{ticket!.Item.TicketId}\"";

                return Results.StatusCode((int)HttpStatusCode.Unauthorized);
            }).RequireAuthorization();
        });
    }
}
