namespace SimpleAuth.AcceptanceTests
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using SimpleAuth;
    using SimpleAuth.Repositories;
    using SimpleAuth.Sms;
    using System;
    using System.Net.Http;
    using System.Threading;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Sms.Ui;
    using SimpleAuth.UI;

    public class ServerStartup
    {
        private readonly SimpleAuthOptions _options;
        private readonly SharedContext _context;

        public ServerStartup(SharedContext context)
        {
            IdentityModelEventSource.ShowPII = true;
            var mockConfirmationCodeStore = new Mock<IConfirmationCodeStore>();
            mockConfirmationCodeStore.Setup(x => x.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockConfirmationCodeStore.Setup(x => x.Remove(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockConfirmationCodeStore.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new ConfirmationCode
                    {
                        ExpiresIn = TimeSpan.FromDays(10).TotalSeconds,
                        IssueAt = DateTimeOffset.UtcNow,
                        Subject = "phone",
                        Value = "123"
                    });
            _options = new SimpleAuthOptions
            {
                AdministratorRoleDefinition = default,
                JsonWebKeys = sp =>
                {
                    var keyset = new[] { context.SignatureKey, context.EncryptionKey }.ToJwks();
                    return new InMemoryJwksRepository(keyset, keyset);
                },
                ConfirmationCodes = sp => mockConfirmationCodeStore.Object,
                Clients =
                    sp => new InMemoryClientRepository(
                        new TestHttpClientFactory(context.Client),
                        new InMemoryScopeRepository(),
                        new Mock<ILogger<InMemoryClientRepository>>().Object,
                        DefaultStores.Clients(context)),
                Scopes = sp => new InMemoryScopeRepository(DefaultStores.Scopes()),
                Consents = sp => new InMemoryConsentRepository(DefaultStores.Consents()),
                Users = sp => new InMemoryResourceOwnerRepository(string.Empty, DefaultStores.Users()),
                ClaimsIncludedInUserCreation = new[] { "acceptance_test" },
            };
            _context = context;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<HttpClient>(x => { }).AddHttpMessageHandler(d => new TestDelegatingHandler(_context.Handler));
            services.AddTransient(sp => _context.Client);
            var mockSmsClient = new Mock<ISmsClient>();
            mockSmsClient.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((true, null));

            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            services.AddSimpleAuth(
                    _options,
                    new[]
                    {
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        JwtBearerDefaults.AuthenticationScheme,
                    },
                    assemblyTypes: new[] { typeof(IDefaultUi), typeof(IDefaultSmsUi) })
                .AddSmsAuthentication(mockSmsClient.Object);
            services.AddLogging().AddAccountFilter();
            services.AddAuthentication(
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
                    cfg =>
                    { });

            services.AddUmaClient(new Uri("http://localhost/"));
            services.ConfigureOptions<JwtBearerPostConfigureOptions>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSimpleAuthMvc(applicationTypes: typeof(IDefaultUi));
        }
    }
}
