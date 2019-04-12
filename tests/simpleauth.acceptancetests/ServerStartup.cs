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
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Sms;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
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
                        DefaultStores.Clients(context)),
                Scopes = sp => new InMemoryScopeRepository(DefaultStores.Scopes()),
                Consents = sp => new InMemoryConsentRepository(DefaultStores.Consents()),
                Users = sp => new InMemoryResourceOwnerRepository(DefaultStores.Users()),
                UserClaimsToIncludeInAuthToken = new[]
                {
                    new Regex($"^{OpenIdClaimTypes.Subject}$", RegexOptions.Compiled),
                    new Regex($"^{OpenIdClaimTypes.Role}$", RegexOptions.Compiled),
                    new Regex($"^{OpenIdClaimTypes.Name}$", RegexOptions.Compiled),
                    new Regex($"^acceptance_test$", RegexOptions.Compiled)
                },
                ClaimsIncludedInUserCreation = new[] { "acceptance_test" }
            };
            _context = context;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // 1. Add the dependencies needed to enable CORS
            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            // 2. Configure server
            services.AddSimpleAuth(_options).AddLogging().AddAccountFilter().AddSingleton(sp => _context.Client);
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
                            ValidateAudience = false
                        };
                        //cfg.TokenValidationParameters = new NoOpTokenValidationParameters(_context);
                    });

            services.AddAuthorization(
                opt => { opt.AddAuthPolicies(DefaultSchema, JwtBearerDefaults.AuthenticationScheme); });

            var mockSmsClient = new Mock<ISmsClient>();
            mockSmsClient.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, null));
            var mvc = services.AddMvc()
                //.AddApplicationPart(typeof(TestController).Assembly)
                .AddSmsAuthentication(mockSmsClient.Object);

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication()
                .UseCors("AllowAll")
                .UseSimpleAuthExceptionHandler()
                .UseSimpleAuthExceptionHandler()
                .UseSimpleAuthMvc();
        }
    }
}
