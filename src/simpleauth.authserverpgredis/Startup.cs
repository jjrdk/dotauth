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
    using System.Net;
    using System.Security.Claims;
    using Amazon;
    using Amazon.Runtime;
    using Baseline;
    using Marten;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;

    using SimpleAuth;
    using SimpleAuth.Extensions;
    using SimpleAuth.Sms;
    using SimpleAuth.Sms.Ui;
    using SimpleAuth.Stores.Marten;
    using SimpleAuth.Stores.Redis;
    using SimpleAuth.UI;
    using StackExchange.Redis;

    internal class Startup
    {
        private const string SimpleAuthScheme = "simpleauth";
        private const string DefaultGoogleScopes = "openid,profile,email";
        private readonly IConfiguration _configuration;
        private readonly SimpleAuthOptions _options;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _ = bool.TryParse(_configuration["REDIRECT"], out var redirect);
            _options = new SimpleAuthOptions
            {
                RedirectToLogin = redirect,
                ApplicationName = _configuration["SERVER:NAME"] ?? "SimpleAuth",
                Users = sp => new MartenResourceOwnerStore(sp.GetRequiredService<IDocumentSession>),
                Clients =
                    sp => new MartenClientStore(sp.GetRequiredService<IDocumentSession>),
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
                Tickets = sp => new RedisTicketStore(sp.GetRequiredService<IDatabaseAsync>(), _options.TicketLifeTime),
                Tokens =
                    sp => new RedisTokenStore(
                        sp.GetRequiredService<IDatabaseAsync>()),
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
            services.AddHttpClient();
            services.AddSingleton<IDocumentStore>(
                provider =>
                {
                    var options = new SimpleAuthMartenOptions(
                        _configuration["DB:CONNECTIONSTRING"],
                        new MartenLoggerFacade(provider.GetService<ILogger<MartenLoggerFacade>>()));
                    return new DocumentStore(options);
                });
            services.AddTransient(sp => sp.GetService<IDocumentStore>().LightweightSession());

            services.AddSingleton(ConnectionMultiplexer.Connect(_configuration["DB:REDISCONFIG"]));
            services.AddTransient(sp => sp.GetRequiredService<ConnectionMultiplexer>().GetDatabase());
            services.AddTransient<IDatabaseAsync>(sp => sp.GetRequiredService<ConnectionMultiplexer>().GetDatabase());

            services.Configure<ForwardedHeadersOptions>(
                options => { options.ForwardedHeaders = ForwardedHeaders.All; });
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
                .AddLogging(log => { log.AddSimpleConsole(o => { o.IncludeScopes = true; }); })
                .AddAuthentication(
                    options =>
                    {
                        options.DefaultScheme = CookieNames.CookieName;
                        options.DefaultChallengeScheme = SimpleAuthScheme;
                    })
                .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; })
                .AddOAuth(SimpleAuthScheme, '_' + SimpleAuthScheme, options => { })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    {
                        cfg.Authority = _configuration["OAUTH:AUTHORITY"];
                        cfg.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidIssuers = _configuration["OAUTH:VALIDISSUERS"]
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .ToArray()
                        };

                        var allowHttp = bool.TryParse(_configuration["SERVER:ALLOWHTTP"], out var ah) && ah;
                        cfg.RequireHttpsMetadata = !allowHttp;
                    });
            services.ConfigureOptions<ConfigureOAuthOptions>();

            if (!string.IsNullOrWhiteSpace(_configuration["GOOGLE:CLIENTID"]))
            {
                services.AddAuthentication(CookieNames.ExternalCookieName)
                    .AddCookie(CookieNames.ExternalCookieName)
                    .AddGoogle(
                        opts =>
                        {
                            opts.AccessType = "offline";
                            opts.ClientId = _configuration["GOOGLE:CLIENTID"];
                            opts.ClientSecret = _configuration["GOOGLE:CLIENTSECRET"];
                            opts.SignInScheme = CookieNames.ExternalCookieName;
                            var scopes = _configuration["GOOGLE:SCOPES"] ?? DefaultGoogleScopes;
                            foreach (var scope in scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim()))
                            {
                                opts.Scope.Add(scope);
                            }
                        });
            }

            if (!string.IsNullOrWhiteSpace(_configuration["AMAZON:ACCESSKEY"])
                && !string.IsNullOrWhiteSpace(_configuration["AMAZON:SECRETKEY"]))
            {
                services.AddSimpleAuth(
                        _options,
                        new[] { CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, SimpleAuthScheme },
                        assemblies: new[]
                        {
                            (GetType().Namespace, GetType().Assembly),
                            (typeof(IDefaultUi).Namespace, typeof(IDefaultUi).Assembly),
                            (typeof(IDefaultSmsUi).Namespace, typeof(IDefaultSmsUi).Assembly)
                        })
                    .AddSmsAuthentication(
                        new AwsSmsClient(
                            new BasicAWSCredentials(
                                _configuration["AMAZON:ACCESSKEY"],
                                _configuration["AMAZON:SECRETKEY"]),
                            RegionEndpoint.EUNorth1,
                            Globals.ApplicationName));
            }
            else
            {
                services.AddSimpleAuth(
                    _options,
                    new[] { CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, SimpleAuthScheme },
                    assemblies: new[]
                    {
                        (GetType().Namespace, GetType().Assembly),
                        (typeof(IDefaultUi).Namespace, typeof(IDefaultUi).Assembly)
                    });
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            //app.Seed();
            var knownProxies = Array.Empty<IPAddress>();
            if (!string.IsNullOrWhiteSpace(_configuration["KNOWN_PROXIES"]))
            {
                knownProxies = _configuration["KNOWN_PROXIES"]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(IPAddress.Parse)
                    .ToArray();
            }

            var pathBase = _configuration["PATHBASE"];
            if (!string.IsNullOrWhiteSpace(pathBase))
            {
                app = app.UsePathBase(pathBase);
            }
            app.UseResponseCompression()
                .UseSimpleAuthMvc(
                    x => { x.KnownProxies.AddRange(knownProxies); },
                    applicationTypes: typeof(IDefaultUi));
        }
    }
}
