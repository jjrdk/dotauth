namespace SimpleAuth.Stores.Marten.AcceptanceTests
{
    using global::Marten;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Npgsql;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Repositories;
    using System;
    using Microsoft.Extensions.Configuration;

    public class ServerStartup : IStartup
    {
        private const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;
        private readonly SimpleAuthOptions _martenOptions;
        private readonly SharedContext _context;
        private readonly string _connectionString;
        private readonly string _schemaName;

        public ServerStartup(SharedContext context, IConfiguration configuration)
        {
            TestData.ConnectionString = configuration["Db:ConnectionString"];
            _martenOptions = new SimpleAuthOptions
            {
                Clients =
                    sp => new MartenClientStore(
                        sp.GetService<Func<IDocumentSession>>(),
                        sp.GetService<IScopeStore>(),
                        context.Client,
                        JsonConvert.DeserializeObject<Uri[]>),
                JsonWebKeys = sp =>
                {
                    var keyset = new[] { context.SignatureKey, context.EncryptionKey }.ToJwks();
                    return new InMemoryJwksRepository(keyset, keyset);
                },
                Scopes = sp => new MartenScopeRepository(sp.GetService<Func<IDocumentSession>>()),
                Consents = sp => new MartenConsentRepository(sp.GetService<Func<IDocumentSession>>()),
                Users = sp => new MartenResourceOwnerStore(sp.GetService<Func<IDocumentSession>>()),
                UserClaimsToIncludeInAuthToken = new[] { "sub", "role", "name" }
            };
            _context = context;
            _connectionString = TestData.ConnectionString;
            var builder = new NpgsqlConnectionStringBuilder { ConnectionString = TestData.ConnectionString };
            _schemaName = builder.SearchPath ?? "public";
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IDocumentStore>(
                provider => new DocumentStore(new SimpleAuthMartenOptions(_connectionString, _schemaName)));
            services.AddTransient<Func<IDocumentSession>>(
                sp =>
                {
                    var store = sp.GetService<IDocumentStore>();
                    return () => store.LightweightSession();
                });
            // 1. Add the dependencies needed to enable CORS
            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            // 2. Configure server
            services.AddSimpleAuth(_martenOptions)
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
}
