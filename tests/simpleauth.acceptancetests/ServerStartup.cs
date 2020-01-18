namespace SimpleAuth.AcceptanceTests
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using SimpleAuth;
    using SimpleAuth.Extensions;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Sms;
    using System;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared.Repositories;

    public class ServerStartup : IStartup
    {
        private const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;
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
                        IssueAt = DateTime.UtcNow,
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
                HttpClientFactory = () => _context.Client
            };
            _context = context;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var mockSmsClient = new Mock<ISmsClient>();
            mockSmsClient.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, null));

            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            services.AddSimpleAuth(_options, new[] { DefaultSchema, JwtBearerDefaults.AuthenticationScheme }).AddSmsAuthentication(mockSmsClient.Object);
            services.AddLogging().AddAccountFilter();
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
                        cfg.BackchannelHttpHandler = _context.Handler;
                        cfg.RequireHttpsMetadata = false;
                        cfg.Authority = _context.Client.BaseAddress.AbsoluteUri;
                        cfg.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidIssuer = "https://localhost"
                        };
                    });

            services.AddAuthorization(
                opt => { opt.AddAuthPolicies(DefaultSchema, JwtBearerDefaults.AuthenticationScheme); });

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSimpleAuthMvc();
        }
    }
}
