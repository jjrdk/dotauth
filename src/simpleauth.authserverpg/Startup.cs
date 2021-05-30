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

namespace SimpleAuth.AuthServerPg
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using SimpleAuth;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using Amazon;
    using Amazon.Runtime;
    using Baseline;
    using Marten;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Extensions;
    using SimpleAuth.Sms;
    using SimpleAuth.Sms.Ui;
    using SimpleAuth.Stores.Marten;
    using SimpleAuth.UI;
    using Aes = System.Security.Cryptography.Aes;

    public class Startup
    {
        private const string SimpleAuthScheme = "simpleauth";
        private const string DefaultGoogleScopes = "openid,profile,email";
        private readonly IConfiguration _configuration;
        private readonly SimpleAuthOptions _options;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _ = bool.TryParse(_configuration[ConfigurationValues.ServerRedirect], out var redirect);
            var allowHttp = bool.TryParse(_configuration[ConfigurationValues.AllowHttp], out var ah) && ah;
            var salt = _configuration["SALT"] ?? string.Empty;
            Func<IServiceProvider, IDataProtector>? dataProtector =
                !string.IsNullOrWhiteSpace(_configuration["IV"]) && !string.IsNullOrWhiteSpace(_configuration["KEY"])
                    ? _ =>
                    {
                        var symmetricAlgorithm = Aes.Create();
                        symmetricAlgorithm.IV = Convert.FromBase64String(_configuration["IV"]);
                        symmetricAlgorithm.Key = Convert.FromBase64String(_configuration["KEY"]);
                        symmetricAlgorithm.Padding = PaddingMode.ISO10126;
                        return new SymmetricDataProtector(symmetricAlgorithm);
                    }
            : null;
            _options =
                new
                    SimpleAuthOptions(
                        salt,
                        ticketLifetime: TimeSpan.FromDays(7),
                        claimsIncludedInUserCreation: new[]
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
                        })
                {
                    DataProtector = dataProtector,
                    AllowHttp = allowHttp,
                    RedirectToLogin = redirect,
                    ApplicationName = _configuration[ConfigurationValues.ServerName] ?? "SimpleAuth",
                    Users = sp => new MartenResourceOwnerStore(salt, sp.GetRequiredService<IDocumentSession>),
                    Clients = sp => new MartenClientStore(sp.GetRequiredService<IDocumentSession>),
                    Scopes = sp => new MartenScopeRepository(sp.GetRequiredService<IDocumentSession>),
                    AccountFilters = sp => new MartenFilterStore(sp.GetRequiredService<IDocumentSession>),
                    AuthorizationCodes =
                            sp => new MartenAuthorizationCodeStore(sp.GetRequiredService<IDocumentSession>),
                    ConfirmationCodes =
                            sp => new MartenConfirmationCodeStore(sp.GetRequiredService<IDocumentSession>),
                    DeviceAuthorizations = sp => new MartenDeviceAuthorizationStore(sp.GetRequiredService<IDocumentSession>),
                    Consents = sp => new MartenConsentRepository(sp.GetRequiredService<IDocumentSession>),
                    JsonWebKeys = sp => new MartenJwksRepository(sp.GetRequiredService<IDocumentSession>),
                    Tickets = sp => new MartenTicketStore(sp.GetRequiredService<IDocumentSession>),
                    Tokens = sp => new MartenTokenStore(sp.GetRequiredService<IDocumentSession>),
                    ResourceSets = sp => new MartenResourceSetRepository(sp.GetRequiredService<IDocumentSession>),
                    EventPublisher = sp =>
                        new LogEventPublisher(sp.GetRequiredService<ILogger<LogEventPublisher>>())
                };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<IDocumentStore>(
                    provider =>
                    {
                        var options = new SimpleAuthMartenOptions(
                            _configuration[ConfigurationValues.ConnectionString],
                            new MartenLoggerFacade(provider.GetRequiredService<ILogger<MartenLoggerFacade>>()));
                        return new DocumentStore(options);
                    })
                .AddTransient(sp => sp.GetRequiredService<IDocumentStore>().LightweightSession())
                .AddResponseCompression(
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
                .AddLogging(
                    log =>
                    {
                        log.AddJsonConsole(
                            o =>
                            {
                                o.IncludeScopes = true;
                                o.UseUtcTimestamp = true;
                            });
                    })
                .AddAuthentication(
                    options =>
                    {
                        options.DefaultScheme = CookieNames.CookieName;
                        options.DefaultChallengeScheme = SimpleAuthScheme;
                    })
                .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; })
                .AddOAuth(SimpleAuthScheme, '_' + SimpleAuthScheme, _ => { })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    {
                        cfg.Authority = _configuration[ConfigurationValues.OauthAuthority];
                        cfg.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidIssuers = _configuration[ConfigurationValues.OauthValidIssuers]
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .ToArray()
                        };

                        cfg.RequireHttpsMetadata = !_options.AllowHttp;
                    });
            services.ConfigureOptions<ConfigureOAuthOptions>()
                .AddHealthChecks()
                .AddNpgSql(_configuration[ConfigurationValues.ConnectionString], failureStatus: HealthStatus.Unhealthy);

            if (!string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.GoogleClientId]))
            {
                services.AddAuthentication(CookieNames.ExternalCookieName)
                    .AddCookie(CookieNames.ExternalCookieName)
                    .AddGoogle(
                        opts =>
                        {
                            opts.AccessType = "offline";
                            opts.ClientId = _configuration[ConfigurationValues.GoogleClientId];
                            opts.ClientSecret = _configuration[ConfigurationValues.GoogleClientSecret];
                            opts.SignInScheme = CookieNames.ExternalCookieName;
                            var scopes = _configuration[ConfigurationValues.GoogleScopes] ?? DefaultGoogleScopes;
                            foreach (var scope in scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim()))
                            {
                                opts.Scope.Add(scope);
                            }
                        });
            }

            if (!string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.AmazonAccessKey])
                && !string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.AmazonSecretKey]))
            {
                services.AddSimpleAuth(
                        _options,
                        new[] { CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, SimpleAuthScheme },
                        assemblyTypes: new[] { GetType(), typeof(IDefaultUi), typeof(IDefaultSmsUi) })
                    .AddSmsAuthentication(
                        new AwsSmsClient(
                            new BasicAWSCredentials(
                                _configuration[ConfigurationValues.AmazonAccessKey],
                                _configuration[ConfigurationValues.AmazonSecretKey]),
                            RegionEndpoint.EUNorth1,
                            Globals.ApplicationName));
            }
            else
            {
                services.AddSimpleAuth(
                    _options,
                    new[] { CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, SimpleAuthScheme },
                    assemblyTypes: new[] { GetType(), typeof(IDefaultUi) });
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            var knownProxies = Array.Empty<IPAddress>();
            if (!string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.KnownProxies]))
            {
                knownProxies = _configuration[ConfigurationValues.KnownProxies]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(IPAddress.Parse)
                    .ToArray();
            }

            var pathBase = _configuration[ConfigurationValues.PathBase];
            if (!string.IsNullOrWhiteSpace(pathBase))
            {
                app = app.UsePathBase(pathBase);
            }

            app.UseResponseCompression()
                .UseSimpleAuthMvc(x => { x.KnownProxies.AddRange(knownProxies); }, applicationTypes: typeof(IDefaultUi))
                .UseEndpoints(endpoint => { endpoint.MapHealthChecks("/health"); });
        }
    }
}
