namespace SimpleAuth.AcceptanceTests
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth;
    using SimpleAuth.Extensions;
    using SimpleAuth.Repositories;
    using System;

    public class ServerStartup : IStartup
    {
        private const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;
        private readonly SimpleAuthOptions _options;
        private readonly SharedContext _context;

        public ServerStartup(SharedContext context)
        {
            _options = new SimpleAuthOptions
            {
                JsonWebKeys = sp =>
                {
                    var keyset = new[] { context.SignatureKey, context.EncryptionKey }.ToJwks();
                    return new InMemoryJwksRepository(keyset, keyset);
                },
                Clients =
                    sp => new InMemoryClientRepository(
                        context.Client,
                        new InMemoryScopeRepository(),
                        DefaultStores.Clients(context)),
                Scopes = sp=>new InMemoryScopeRepository(DefaultStores.Scopes()),
                Consents = sp => new InMemoryConsentRepository(DefaultStores.Consents()),
                Users = sp => new InMemoryResourceOwnerRepository(DefaultStores.Users()),
                UserClaimsToIncludeInAuthToken = new[] { "sub", "role", "name" }
            };
            _context = context;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // 1. Add the dependencies needed to enable CORS
            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            // 2. Configure server
            services.AddSimpleAuth(_options)
                .AddLogging()
                .AddAccountFilter()
                .AddSingleton(sp => _context.Client);
            services.AddAuthentication(
                    cfg =>
                    {
                        cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                        cfg.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                        cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddCookie(DefaultSchema)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    {
                        cfg.RequireHttpsMetadata = false;
                        cfg.TokenValidationParameters = new NoOpTokenValidationParameters(_context);
                    });
            services.AddAuthorization(
                opt => { opt.AddAuthPolicies(DefaultSchema, JwtBearerDefaults.AuthenticationScheme); });
            // 3. Configure MVC
            var mvc = services.AddMvc();

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 4. Use simple identity server.
            app.UseSimpleAuthExceptionHandler();
            // 5. Use MVC.
            app.UseMvcWithDefaultRoute();
        }
    }

    internal class NoOpTokenValidationParameters : TokenValidationParameters
    {
        public NoOpTokenValidationParameters(SharedContext context)
        {
            RequireExpirationTime = false;
            RequireSignedTokens = false;
            SaveSigninToken = false;
            ValidateActor = false;
            ValidateAudience = false;
            ValidateIssuer = false;
            ValidateIssuerSigningKey = true;
            ValidateLifetime = false;
            ValidateTokenReplay = false;
            IssuerSigningKey = context.SignatureKey;
            //TestKeys.SecretKey.CreateJwk(
            //JsonWebKeyUseNames.Sig,
            //KeyOperations.Sign,
            //KeyOperations.Verify);
        }
    }
}