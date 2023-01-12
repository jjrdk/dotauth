namespace DotAuth.Stores.Marten.AcceptanceTests;

using System;
using System.Net.Http;
using DotAuth;
using DotAuth.Extensions;
using DotAuth.Repositories;
using DotAuth.Shared.Policies;
using DotAuth.Stores.Marten;
using DotAuth.UI;
using global::Marten;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Weasel.Core;
using Xunit.Abstractions;

public sealed class ServerStartup
{
    private const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;
    private readonly DotAuthOptions _martenOptions;
    private readonly SharedContext _context;
    private readonly string _connectionString;
    private readonly ITestOutputHelper _outputHelper;
    private readonly string _schemaName;

    public ServerStartup(SharedContext context, string connectionString, ITestOutputHelper outputHelper)
    {
        _martenOptions = new DotAuthOptions
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
        var builder = new NpgsqlConnectionStringBuilder { ConnectionString = _connectionString };
        _schemaName = builder.SearchPath ?? "public";
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<HttpClient>(x=>{})
            .AddHttpMessageHandler(() => new TestDelegatingHandler(_context.Handler()));
        services.AddSingleton<IDocumentStore>(
            _ => new DocumentStore(
                new DotAuthMartenOptions(
                    _connectionString,
                    new MartenLoggerFacade(NullLogger<MartenLoggerFacade>.Instance),
                    _schemaName,
                    AutoCreate.CreateOrUpdate)));
        services.AddTransient<Func<IDocumentSession>>(
            sp =>
            {
                var store = sp.GetRequiredService<IDocumentStore>();
                return () => store.LightweightSession("test");
            });

        services.AddCors(
            options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        services.AddDotAuth(
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
        
        services.AddUmaClient(new Uri("http://localhost/"));
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseDotAuthMvc(applicationTypes: typeof(IDefaultUi));
    }
}