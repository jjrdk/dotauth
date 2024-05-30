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

namespace DotAuth.AuthServerPgRedis;

using System;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Amazon;
using Amazon.Runtime;
using DotAuth;
using DotAuth.Extensions;
using DotAuth.Sms;
using DotAuth.Sms.Ui;
using DotAuth.Stores.Marten;
using DotAuth.Stores.Redis;
using DotAuth.UI;
using Marten;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

internal sealed class Startup
{
    private const string DotAuthScheme = "dotauth";
    private const string DefaultScopes = "openid,profile,email";
    private readonly IConfiguration _configuration;
    private readonly DotAuthConfiguration _dotAuthConfiguration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
        _ = bool.TryParse(_configuration["REDIRECT"], out var redirect);
        var salt = _configuration["SALT"] ?? string.Empty;
        var allowHttp = bool.TryParse(_configuration["SERVER:ALLOWHTTP"], out var ah) && ah;
        Func<IServiceProvider, IDataProtector>? dataProtector =
            !string.IsNullOrWhiteSpace(_configuration["IV"]) && !string.IsNullOrWhiteSpace(_configuration["KEY"])
                ? _ =>
                {
                    var symmetricAlgorithm = Aes.Create();
                    symmetricAlgorithm.IV = Convert.FromBase64String(_configuration["IV"] ?? "");
                    symmetricAlgorithm.Key = Convert.FromBase64String(_configuration["KEY"] ?? "");
                    symmetricAlgorithm.Padding = PaddingMode.ISO10126;
                    return new SymmetricDataProtector(symmetricAlgorithm);
                }
                : null;
        _dotAuthConfiguration = new DotAuthConfiguration(salt)
        {
            AllowHttp = allowHttp,
            DataProtector = dataProtector,
            RedirectToLogin = redirect,
            ApplicationName = _configuration["SERVER:NAME"] ?? "DotAuth",
            Users = sp => new MartenResourceOwnerStore(salt, sp.GetRequiredService<IDocumentSession>),
            Clients =
                sp => new MartenClientStore(sp.GetRequiredService<IDocumentSession>,
                    sp.GetRequiredService<ILogger<MartenClientStore>>()),
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
            DeviceAuthorizations = sp => new MartenDeviceAuthorizationStore(sp.GetRequiredService<IDocumentSession>),
            JsonWebKeys = sp => new MartenJwksRepository(sp.GetRequiredService<IDocumentSession>),
            Tickets = sp =>
                new RedisTicketStore(sp.GetRequiredService<IDatabaseAsync>(), _dotAuthConfiguration!.TicketLifeTime),
            Tokens =
                sp => new RedisTokenStore(
                    sp.GetRequiredService<IDatabaseAsync>()),
            ResourceSets = sp => new MartenResourceSetRepository(sp.GetRequiredService<IDocumentSession>,
                sp.GetRequiredService<ILogger<MartenResourceSetRepository>>()),
            EventPublisher = sp => new LogEventPublisher(sp.GetRequiredService<ILogger<LogEventPublisher>>()),
            ClaimsIncludedInUserCreation =
            [
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
            ]
        };
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IDocumentStore>(
            provider =>
            {
                var options = new DotAuthMartenOptions(
                    _configuration["DB:CONNECTIONSTRING"] ?? "",
                    new MartenLoggerFacade(provider.GetRequiredService<ILogger<MartenLoggerFacade>>()));
                return new DocumentStore(options);
            });
        services.AddTransient(sp => sp.GetRequiredService<IDocumentStore>().LightweightSession());

        services.AddSingleton(ConnectionMultiplexer.Connect(_configuration["DB:REDISCONFIG"] ?? ""));
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
                    options.DefaultChallengeScheme = DotAuthScheme;
                })
            .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; })
            .AddOAuth(DotAuthScheme, '_' + DotAuthScheme, _ => { })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                cfg =>
                {
                    cfg.Authority = _configuration["OAUTH:AUTHORITY"];
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidIssuers = (_configuration["OAUTH:VALIDISSUERS"] ?? "")
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .ToArray()
                    };

                    cfg.RequireHttpsMetadata = !_dotAuthConfiguration.AllowHttp;
                });
        services.ConfigureOptions<ConfigureOAuthOptions>();

        if (!string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.GoogleClientId]) &&
            !string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.GoogleClientSecret]))
        {
            services.AddAuthentication(CookieNames.ExternalCookieName)
                .AddCookie(CookieNames.ExternalCookieName)
                .AddGoogle(
                    opts =>
                    {
                        opts.AccessType = "offline";
                        opts.ClientId = _configuration["GOOGLE:CLIENTID"] ?? "";
                        opts.ClientSecret = _configuration["GOOGLE:CLIENTSECRET"] ?? "";
                        opts.SignInScheme = CookieNames.ExternalCookieName;
                        var scopes = _configuration["GOOGLE:SCOPES"] ?? DefaultScopes;
                        foreach (var scope in scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim()))
                        {
                            opts.Scope.Add(scope);
                        }
                    });
        }

        if (!string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.MsClientId])
         && !string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.MsClientSecret]))
        {
            services.AddAuthentication(CookieNames.ExternalCookieName)
                .AddMicrosoftAccount(
                    opts =>
                    {
                        opts.ClientId = _configuration[ConfigurationValues.MsClientId]!;
                        opts.ClientSecret = _configuration[ConfigurationValues.MsClientSecret]!;
                        opts.UsePkce = true;
                        var scopes = _configuration[ConfigurationValues.MsScopes] ?? DefaultScopes;
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
            services.AddDotAuthServer(
                    _dotAuthConfiguration,
                    [CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, DotAuthScheme])
                .AddDotAuthUi(GetType(), typeof(IDefaultUi), typeof(IDefaultSmsUi))
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
            services.AddDotAuthServer(
                    _dotAuthConfiguration,
                    [CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, DotAuthScheme])
                .AddDotAuthUi(GetType(), typeof(IDefaultUi));
        }
    }

    public void Configure(IApplicationBuilder app)
    {
        //app.Seed();
        var knownProxies = Array.Empty<IPAddress>();
        if (!string.IsNullOrWhiteSpace(_configuration["KNOWN_PROXIES"]))
        {
            knownProxies = (_configuration["KNOWN_PROXIES"] ?? "")
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
            .UseDotAuthServer(
                x =>
                {
                    foreach (var proxy in knownProxies)
                    {
                        x.KnownProxies.Add(proxy);
                    }
                },
                applicationTypes: typeof(IDefaultUi));
    }
}
