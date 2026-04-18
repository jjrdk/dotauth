namespace DotAuth.Stores.Redis.AcceptanceTests;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
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
using DotAuth.Stores.Redis;
using DotAuth.UI;
using global::Marten;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;
using Xunit;

internal sealed class ServerStartup
{
    private const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;
    private readonly DotAuthConfiguration _martenConfiguration;
    private readonly SharedContext _context;
    private readonly string _connectionString;
    private readonly string _redisConnectionString;
    private readonly ITestOutputHelper _outputHelper;
    private readonly string _schemaName;

    public ServerStartup(
        SharedContext context,
        string connectionString,
        string redisHost,
        ITestOutputHelper outputHelper)
    {
        _martenConfiguration = new DotAuthConfiguration
        {
            AdministratorRoleDefinition = default,
            AuthorizationCodes =
                sp => new RedisAuthorizationCodeStore(
                    sp.GetRequiredService<IDatabaseAsync>(),
                    _martenConfiguration!.AuthorizationCodeValidityPeriod),
            Clients = sp => new MartenClientStore(sp.GetRequiredService<Func<IDocumentSession>>(),
                sp.GetRequiredService<ILogger<MartenClientStore>>()),
            ConfirmationCodes =
                sp => new RedisConfirmationCodeStore(
                    sp.GetRequiredService<IDatabaseAsync>(),
                    _martenConfiguration!.RptLifeTime),
            Consents = sp => new RedisConsentStore(sp.GetRequiredService<IDatabaseAsync>()),
            JsonWebKeys = _ =>
            {
                var keyset = new[] { context.SignatureKey, context.EncryptionKey }.ToJwks();
                return new InMemoryJwksRepository(keyset, keyset);
            },
            Scopes = sp => new MartenScopeRepository(sp.GetRequiredService<Func<IDocumentSession>>()),
            Users = sp => new MartenResourceOwnerStore(string.Empty, sp.GetRequiredService<Func<IDocumentSession>>()),
            ResourceSets =
                sp => new MartenResourceSetRepository(
                    sp.GetRequiredService<Func<IDocumentSession>>(),
                    sp.GetRequiredService<ILogger<MartenResourceSetRepository>>()),
            Tickets =
                sp => new RedisTicketStore(sp.GetRequiredService<IDatabaseAsync>(),
                    _martenConfiguration!.TicketLifeTime),
            Tokens = sp => new RedisTokenStore(sp.GetRequiredService<IDatabaseAsync>()),
            DevicePollingInterval = TimeSpan.FromSeconds(3),
            DeviceAuthorizationLifetime = TimeSpan.FromSeconds(5)
        };
        _context = context;
        _connectionString = connectionString;
        _redisConnectionString = redisHost;
        _outputHelper = outputHelper;
        var builder = new NpgsqlConnectionStringBuilder { ConnectionString = _connectionString };
        _schemaName = builder.SearchPath ?? "public";
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<HttpClient>()
            .AddHttpMessageHandler(() => new TestDelegatingHandler(_context.Handler!()));
        var db = new Random(DateTime.UtcNow.Millisecond).Next(16);
        services.AddSingleton(ConnectionMultiplexer.Connect(_redisConnectionString));
        services.AddTransient<IDatabaseAsync>(sp => sp.GetRequiredService<ConnectionMultiplexer>().GetDatabase(db));
        services.AddSingleton<IDocumentStore>(provider => new DocumentStore(
            new DotAuthMartenOptions(
                _connectionString,
                new MartenLoggerFacade(provider.GetRequiredService<ILogger<MartenLoggerFacade>>()),
                _schemaName)));
        services.AddTransient<Func<IDocumentSession>>(sp =>
        {
            var store = sp.GetRequiredService<IDocumentStore>();
            return () => store.LightweightSession();
        });
        // 1. Add the dependencies needed to enable CORS
        services.AddCors(options =>
            options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        // 2. Configure server
        services.AddDotAuthServer(_martenConfiguration, [DefaultSchema, JwtBearerDefaults.AuthenticationScheme])
            .AddDotAuthUi(typeof(IDefaultUi));
        services
#if DEBUG
            .AddLogging(l => l.AddXUnit(_outputHelper))
#endif
            .AddAccountFilter()
            .AddSingleton(_ => _context.Client!);
        services.AddAuthentication(cfg =>
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
                    cfg.Events = new JwtBearerEvents { OnAuthenticationFailed = _ => Task.CompletedTask };
                    cfg.RequireHttpsMetadata = false;
                    cfg.TokenValidationParameters = new NoOpTokenValidationParameters(_context);
                });
        services.AddUmaClient(new Uri("http://localhost"));
    }

#pragma warning disable CA1822 // Mark members as static
    public void Configure(IApplicationBuilder app)
#pragma warning restore CA1822 // Mark members as static
    {
        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(() =>
        {
            var disposable = app.ApplicationServices.GetService<ConnectionMultiplexer>();
            disposable?.Dispose();
        });
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
                var request = new PermissionRequest { ResourceSetId = id, Scopes = ["api1"] };
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
