// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.AuthServerPgRedis
{
    using System;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using Marten;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;

    using Newtonsoft.Json;

    using SimpleAuth;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Sms.Ui;
    using SimpleAuth.Stores.Marten;
    using SimpleAuth.Stores.Redis;
    using SimpleAuth.UI;
    using StackExchange.Redis;

    internal class Startup
    {
        private static readonly string DefaultGoogleScopes = "openid,profile,email";
        private readonly IConfiguration _configuration;
        private readonly SimpleAuthOptions _options;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _options = new SimpleAuthOptions
            {
                ApplicationName = _configuration["ApplicationName"] ?? "SimpleAuth",
                Users = sp => new MartenResourceOwnerStore(sp.GetRequiredService<IDocumentSession>),
                Clients =
                    sp => new MartenClientStore(
                        sp.GetRequiredService<IDocumentSession>,
                        sp.GetRequiredService<IScopeStore>(),
                        sp.GetRequiredService<HttpClient>(),
                        JsonConvert.DeserializeObject<Uri[]>),
                Scopes = sp => new MartenScopeRepository(sp.GetRequiredService<IDocumentSession>),
                AccountFilters = sp => new MartenFilterStore(sp.GetRequiredService<IDocumentSession>),
                AuthorizationCodes =
                    sp => new RedisAuthorizationCodeStore(
                        sp.GetRequiredService<IDatabaseAsync>(),
                        TimeSpan.FromMinutes(30)),
                ConfirmationCodes =
                    sp => new RedisConfirmationCodeStore(
                        sp.GetRequiredService<IDatabaseAsync>(),
                        TimeSpan.FromMinutes(30)),
                Consents = sp => new RedisConsentStore(sp.GetRequiredService<IDatabaseAsync>()),
                JsonWebKeys = sp => new MartenJwksRepository(sp.GetRequiredService<IDocumentSession>),
                Tickets = sp => new RedisTicketStore(sp.GetRequiredService<IDatabaseAsync>()),
                Tokens =
                    sp => new RedisTokenStore(
                        sp.GetRequiredService<IDatabaseAsync>(),
                        sp.GetRequiredService<IJwksStore>()),
                ResourceSets = sp => new MartenResourceSetRepository(sp.GetRequiredService<IDocumentSession>),
                EventPublisher = sp => new LogEventPublisher(sp.GetRequiredService<ILogger<LogEventPublisher>>()),
                ClaimsIncludedInUserCreation = new[]
                {
                    ClaimTypes.Name,
                    ClaimTypes.Uri,
                    ClaimTypes.Country,
                    ClaimTypes.DateOfBirth,
                    ClaimTypes.Email,
                    ClaimTypes.Gender,
                    ClaimTypes.GivenName,
                    ClaimTypes.Locality,
                    ClaimTypes.PostalCode,
                    ClaimTypes.Role,
                    ClaimTypes.StateOrProvince,
                    ClaimTypes.StreetAddress,
                    ClaimTypes.Surname
                }
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<HttpClient>();
            services.AddSingleton<IDocumentStore>(
                provider =>
                {
                    var options = new SimpleAuthMartenOptions(
                        _configuration["ConnectionString"],
                        new MartenLoggerFacade(provider.GetService<ILogger<MartenLoggerFacade>>()));
                    return new DocumentStore(options);
                });
            services.AddTransient(sp => sp.GetService<IDocumentStore>().LightweightSession());

            services.AddSingleton(ConnectionMultiplexer.Connect(_configuration["RedisConfig"]));
            services.AddTransient(sp => sp.GetRequiredService<ConnectionMultiplexer>().GetDatabase());
            services.AddTransient<IDatabaseAsync>(sp => sp.GetRequiredService<ConnectionMultiplexer>().GetDatabase());

            services.AddResponseCompression(
                    x =>
                    {
                        x.EnableForHttps = true;
                        x.Providers.Add(
                            new GzipCompressionProvider(
                                new GzipCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                        x.Providers.Add(
                            new BrotliCompressionProvider(
                                new BrotliCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                    })
                .AddHttpContextAccessor()
                .AddCors(
                    options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()))
                .AddLogging(log => { log.AddConsole(o => { o.IncludeScopes = true; }); });
            services.AddAuthentication(CookieNames.CookieName)
                .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    {
                        cfg.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false, };
#if DEBUG
                        cfg.RequireHttpsMetadata = false;
#endif
                    });
            if (!string.IsNullOrWhiteSpace(_configuration["Google:ClientId"]))
            {
                services.AddAuthentication(CookieNames.ExternalCookieName)
                    .AddCookie(CookieNames.ExternalCookieName)
                    .AddGoogle(
                        opts =>
                        {
                            opts.AccessType = "offline";
                            opts.ClientId = _configuration["Google:ClientId"];
                            opts.ClientSecret = _configuration["Google:ClientSecret"];
                            opts.SignInScheme = CookieNames.ExternalCookieName;
                            var scopes = _configuration["Google:Scopes"] ?? DefaultGoogleScopes;
                            foreach (var scope in scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim()))
                            {
                                opts.Scope.Add(scope);
                            }
                        });
            }

            services.AddSimpleAuth(
                _options,
                new[] { CookieNames.CookieName, CookieNames.ExternalCookieName, JwtBearerDefaults.AuthenticationScheme },
                assemblyTypes: new[] { typeof(IDefaultUi), typeof(IDefaultSmsUi) });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseResponseCompression().UseSimpleAuthMvc(typeof(IDefaultUi));
        }
    }
}
