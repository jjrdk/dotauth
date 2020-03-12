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
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared.Repositories;

    public class ServerStartup
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

        public void ConfigureServices(IServiceCollection services)
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
                    { });

            services.AddAuthorization(
                opt => { opt.AddAuthPolicies(DefaultSchema, JwtBearerDefaults.AuthenticationScheme); });
            services.ConfigureOptions<JwtBearerPostConfigureOptions>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSimpleAuthMvc();
        }
    }

    internal class JwtBearerPostConfigureOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        private const string Bearer = "Bearer ";
        private readonly TestServer _server;

        public JwtBearerPostConfigureOptions(IServer server)
        {
            _server = server as TestServer;
        }

        

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            options.Authority = _server.CreateClient().BaseAddress.AbsoluteUri;
            options.BackchannelHttpHandler = _server.CreateHandler();
            options.RequireHttpsMetadata = false;
            options.Events = new JwtBearerEvents { OnAuthenticationFailed = ctx => throw ctx.Exception };
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = false,
                ValidIssuer = "http://localhost:5000"
            };
            if (string.IsNullOrEmpty(options.TokenValidationParameters.ValidAudience)
                && !string.IsNullOrEmpty(options.Audience))
            {
                options.TokenValidationParameters.ValidAudience = options.Audience;
            }

            if (options.ConfigurationManager == null)
            {
                if (options.Configuration != null)
                {
                    options.ConfigurationManager =
                        new StaticConfigurationManager<OpenIdConnectConfiguration>(options.Configuration);
                }
                else if (!(string.IsNullOrEmpty(options.MetadataAddress) && string.IsNullOrEmpty(options.Authority)))
                {
                    if (string.IsNullOrEmpty(options.MetadataAddress) && !string.IsNullOrEmpty(options.Authority))
                    {
                        options.MetadataAddress = options.Authority;
                        if (!options.MetadataAddress.EndsWith("/", StringComparison.Ordinal))
                        {
                            options.MetadataAddress += "/";
                        }

                        options.MetadataAddress += ".well-known/openid-configuration";
                    }

                    if (options.RequireHttpsMetadata
                        && !options.MetadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
                    }

                    var httpClient = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
                    httpClient.Timeout = options.BackchannelTimeout;
                    httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB

                    options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        options.MetadataAddress,
                        new OpenIdConnectConfigurationRetriever(),
                        new HttpDocumentRetriever(httpClient) { RequireHttps = options.RequireHttpsMetadata });
                }
            }
        }
    }
}
