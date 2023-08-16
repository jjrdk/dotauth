namespace DotAuth.Authentication.AcceptanceTests.Support;

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using DotAuth;
using DotAuth.Extensions;
using DotAuth.Repositories;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using NSubstitute;

public sealed class ServerStartup
{
    private readonly SharedContext _context;
    private readonly DotAuthConfiguration _configuration;

    public ServerStartup(SharedContext context)
    {
        _context = context;
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
            ClaimsIncludedInUserCreation = new[] { "acceptance_test" },
            DeviceAuthorizationLifetime = TimeSpan.FromSeconds(5),
            DevicePollingInterval = TimeSpan.FromSeconds(3)
        };
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<HttpClient>(_ => { })
            .AddHttpMessageHandler(_ => new TestDelegatingHandler(_context.Handler!));
        //services.AddLogging(x => x.AddXunit(_outputHelper));
        services.AddCors(
            options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        services.AddDotAuthServer(
                _configuration,
                new[]
                {
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    JwtBearerDefaults.AuthenticationScheme,
                })
            .AddDotAuthUi(typeof(IDefaultUi));
        services
            .AddAccountFilter()
            .AddAuthentication(
                cfg =>
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

        services.ConfigureOptions<JwtBearerPostConfigureOptions>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseDotAuthServer(applicationTypes: typeof(IDefaultUi));
    }
}
