namespace SimpleAuth.AcceptanceTests
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using SimpleAuth;
    using SimpleAuth.Extensions;
    using SimpleAuth.Repositories;
    using SimpleAuth.Sms;
    using System;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;

    using SimpleAuth.ResourceServer;
    using SimpleAuth.ResourceServer.Authentication;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;

    public class ServerStartup
    {
        //private const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;
        private readonly SimpleAuthOptions _options;
        private readonly SharedContext _context;

        public ServerStartup(SharedContext context)
        {
            IdentityModelEventSource.ShowPII = true;
            var mockConfirmationCodeStore = new Mock<IConfirmationCodeStore>();
            mockConfirmationCodeStore.Setup(x => x.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockConfirmationCodeStore.Setup(x => x.Remove(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockConfirmationCodeStore.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
                JsonWebKeys = sp =>
                {
                    var keyset = new[] { context.SignatureKey, context.EncryptionKey }.ToJwks();
                    return new InMemoryJwksRepository(keyset, keyset);
                },
                ConfirmationCodes = sp => mockConfirmationCodeStore.Object,
                Clients =
                    sp => new InMemoryClientRepository(
                        context.Client,
                        new InMemoryScopeRepository(),
                        new Mock<ILogger<InMemoryClientRepository>>().Object,
                        DefaultStores.Clients(context)),
                Scopes = sp => new InMemoryScopeRepository(DefaultStores.Scopes()),
                Consents = sp => new InMemoryConsentRepository(DefaultStores.Consents()),
                Users = sp => new InMemoryResourceOwnerRepository(DefaultStores.Users()),
                ClaimsIncludedInUserCreation = new[] { "acceptance_test" },
                HttpClientFactory = () => context.Client
            };
            _context = context;
        }

        public void ConfigureServices(IServiceCollection services)
        {
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
                        UmaAuthenticationDefaults.AuthenticationScheme
                    })
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
                .AddUmaTicket(
                    configureOptions: cfg =>
                    {
                        cfg.ResourceSetRequest = r => new[]
                        {
                            new PermissionRequest
                            {
                                ResourceSetId = r.Path.Value.Replace("/data/", string.Empty),
                                Scopes = new[] {"api1"}
                            }
                        };
                        cfg.UmaResourcePaths = new[] { new Regex("/data/.+", RegexOptions.Compiled), };
                        cfg.Authority = new Uri("http://localhost");
                        cfg.BackchannelHttpHandler = _context.Handler;
                        cfg.RequireHttpsMetadata = false;
                        cfg.DiscoveryDocumentUri = new Uri("http://localhost/.well-known/openid-configuration");
                        cfg.ClientId = "clientCredentials";
                        cfg.ClientSecret = "clientCredentials";
                        cfg.TokenValidationParameters = new NoOpTokenValidationParameters(_context);
                    })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    { });

            services.AddUmaClient(new Uri("http://localhost/.well-known/uma2-configuration"));
            services.AddAuthorization(opt => { opt.AddPolicy("uma_ticket", builder => builder.RequireUmaTicket()); });
            services.ConfigureOptions<JwtBearerPostConfigureOptions>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSimpleAuthMvc();
        }
    }
}
