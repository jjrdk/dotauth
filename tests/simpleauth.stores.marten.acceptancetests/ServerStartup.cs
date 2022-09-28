namespace SimpleAuth.Stores.Marten.AcceptanceTests;

using global::Marten;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SimpleAuth.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.UI;
using Weasel.Core;
using Xunit.Abstractions;

public sealed class ServerStartup
{
    private const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;
    private readonly SimpleAuthOptions _martenOptions;
    private readonly SharedContext _context;
    private readonly string _connectionString;
    private readonly ITestOutputHelper _outputHelper;
    private readonly string _schemaName;

    public ServerStartup(SharedContext context, string connectionString, ITestOutputHelper outputHelper)
    {
        _martenOptions = new SimpleAuthOptions
        {
            AdministratorRoleDefinition = default,
            Clients = sp => new MartenClientStore(sp.GetRequiredService<Func<IDocumentSession>>()),
            JsonWebKeys = _ =>
            {
                var keyset = new[] { context.SignatureKey, context.EncryptionKey }.ToJwks();
                return new InMemoryJwksRepository(keyset, keyset);
            },
            Scopes = sp => new MartenScopeRepository(sp.GetRequiredService<Func<IDocumentSession>>()),
            Consents = sp => new MartenConsentRepository(sp.GetRequiredService<Func<IDocumentSession>>()),
            Users = sp => new MartenResourceOwnerStore(string.Empty, sp.GetRequiredService<Func<IDocumentSession>>()),
            DeviceAuthorizationLifetime = TimeSpan.FromSeconds(5),
            DevicePollingInterval = TimeSpan.FromSeconds(3)
        };
        _context = context;
        _connectionString = connectionString;
        _outputHelper = outputHelper;
        var builder = new NpgsqlConnectionStringBuilder { ConnectionString = _connectionString };
        _schemaName = builder.SearchPath ?? "public";
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<HttpClient>()
            .AddHttpMessageHandler(() => new TestDelegatingHandler(_context.Handler()));
        services.AddSingleton<IDocumentStore>(
            _ => new DocumentStore(
                new SimpleAuthMartenOptions(
                    _connectionString,
                    new MartenLoggerFacade(NullLogger<MartenLoggerFacade>.Instance),
                    _schemaName,
                    AutoCreate.None)));
        services.AddTransient<Func<IDocumentSession>>(
            sp =>
            {
                var store = sp.GetRequiredService<IDocumentStore>();
                return () => store.LightweightSession("test");
            });

        services.AddCors(
            options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        services.AddSimpleAuth(
            _martenOptions,
            new[] { DefaultSchema, JwtBearerDefaults.AuthenticationScheme },
            assemblyTypes: typeof(IDefaultUi));
        services.AddLogging(l => l.AddXunit(_outputHelper)).AddAccountFilter().AddSingleton(_ => _context.Client);
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
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseSimpleAuthMvc(applicationTypes: typeof(IDefaultUi));
    }
}