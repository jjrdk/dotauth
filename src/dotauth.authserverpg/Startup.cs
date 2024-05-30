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

namespace DotAuth.AuthServerPg;

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
using DotAuth.UI;
using Marten;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Weasel.Core;

/// <summary>
/// Configuration class.
/// </summary>
public sealed class Startup
{
    private const string DotAuthScheme = "dotauth";
    private const string DefaultScopes = "openid,profile,email";
    private readonly IConfiguration _configuration;
    private readonly DotAuthConfiguration _dotAuthConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
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
                    symmetricAlgorithm.IV = Convert.FromBase64String(_configuration["IV"] ?? "");
                    symmetricAlgorithm.Key = Convert.FromBase64String(_configuration["KEY"] ?? "");
                    symmetricAlgorithm.Padding = PaddingMode.ISO10126;
                    return new SymmetricDataProtector(symmetricAlgorithm);
                }
                : null;
        _dotAuthConfiguration =
            new
                DotAuthConfiguration(
                    salt,
                    ticketLifetime: TimeSpan.FromDays(7),
                    claimsIncludedInUserCreation:
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
                    ])
                {
                    DataProtector = dataProtector,
                    AllowHttp = allowHttp,
                    RedirectToLogin = redirect,
                    ApplicationName = _configuration[ConfigurationValues.ServerName] ?? "DotAuth",
                    Users = sp => new MartenResourceOwnerStore(salt, sp.GetRequiredService<IDocumentSession>),
                    Clients = sp => new MartenClientStore(sp.GetRequiredService<IDocumentSession>,
                        sp.GetRequiredService<ILogger<MartenClientStore>>()),
                    Scopes = sp => new MartenScopeRepository(sp.GetRequiredService<IDocumentSession>),
                    AccountFilters = sp => new MartenFilterStore(sp.GetRequiredService<IDocumentSession>),
                    AuthorizationCodes =
                        sp => new MartenAuthorizationCodeStore(sp.GetRequiredService<IDocumentSession>),
                    ConfirmationCodes =
                        sp => new MartenConfirmationCodeStore(sp.GetRequiredService<IDocumentSession>),
                    DeviceAuthorizations = sp =>
                        new MartenDeviceAuthorizationStore(sp.GetRequiredService<IDocumentSession>),
                    Consents = sp => new MartenConsentRepository(sp.GetRequiredService<IDocumentSession>),
                    JsonWebKeys = sp => new MartenJwksRepository(sp.GetRequiredService<IDocumentSession>),
                    Tickets = sp => new MartenTicketStore(sp.GetRequiredService<IDocumentSession>),
                    Tokens = sp => new MartenTokenStore(sp.GetRequiredService<IDocumentSession>,
                        sp.GetRequiredService<ILogger<MartenTokenStore>>()),
                    ResourceSets = sp => new MartenResourceSetRepository(sp.GetRequiredService<IDocumentSession>,
                        sp.GetRequiredService<ILogger<MartenResourceSetRepository>>()),
                    EventPublisher = sp =>
                        new LogEventPublisher(sp.GetRequiredService<ILogger<LogEventPublisher>>())
                };
    }

    /// <summary>
    /// Configures services.
    /// </summary>
    /// <param name="services">The services to configure or extend.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IDocumentStore>(
                provider =>
                {
                    var options = new DotAuthMartenOptions(
                        _configuration[ConfigurationValues.ConnectionString]!,
                        new MartenLoggerFacade(provider.GetRequiredService<ILogger<MartenLoggerFacade>>()),
                        autoCreate: AutoCreate.CreateOrUpdate);
                    return new DocumentStore(options);
                })
            .AddTransient(sp =>
            {
                var contextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var context = contextAccessor.HttpContext;
                var host = context?.Request.Host.Host ?? "localhost";
                return sp.GetRequiredService<IDocumentStore>().LightweightSession(host);
            })
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
                    options.DefaultChallengeScheme = DotAuthScheme;
                })
            .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; })
            .AddOAuth(DotAuthScheme, $"_{DotAuthScheme}", _ => { })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                cfg =>
                {
                    cfg.Authority = _configuration[ConfigurationValues.OauthAuthority];
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidIssuers = (_configuration[ConfigurationValues.OauthValidIssuers] ?? "")
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .ToArray()
                    };

                    cfg.RequireHttpsMetadata = !_dotAuthConfiguration.AllowHttp;
                });
        services.ConfigureOptions<ConfigureOAuthOptions>()
            .AddHealthChecks()
            .AddNpgSql(_configuration[ConfigurationValues.ConnectionString]!, failureStatus: HealthStatus.Unhealthy);

        if (!string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.GoogleClientId]) &&
            !string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.GoogleClientSecret]))
        {
            services.AddAuthentication(CookieNames.ExternalCookieName)
                .AddCookie(CookieNames.ExternalCookieName)
                .AddGoogle(
                    opts =>
                    {
                        opts.AccessType = "offline";
                        opts.ClientId = _configuration[ConfigurationValues.GoogleClientId]!;
                        opts.ClientSecret = _configuration[ConfigurationValues.GoogleClientSecret]!;
                        var scopes = _configuration[ConfigurationValues.GoogleScopes] ?? DefaultScopes;
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
            services.AddAuthentication(CookieNames.ExternalCookieName).AddMicrosoftAccount(
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

        if (!string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.AmazonAccessKey])
         && !string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.AmazonSecretKey]))
        {
            services.AddDotAuthServer(
                    _dotAuthConfiguration,
                    [CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, DotAuthScheme])
                .AddDotAuthUi(GetType(), typeof(IDefaultUi), typeof(IDefaultSmsUi))
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
            services.AddDotAuthServer(
                    _dotAuthConfiguration,
                    [CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, DotAuthScheme])
                .AddDotAuthUi(GetType(), typeof(IDefaultUi));
        }
    }

    /// <summary>
    /// Configures the application.
    /// </summary>
    /// <param name="app">The app to configure</param>
    public void Configure(IApplicationBuilder app)
    {
        var knownProxies = Array.Empty<IPAddress>();
        if (!string.IsNullOrWhiteSpace(_configuration[ConfigurationValues.KnownProxies]))
        {
            knownProxies = (_configuration[ConfigurationValues.KnownProxies] ?? "")
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
            .Use(
                async (ctx, next) =>
                {
                    var logger = ctx.RequestServices.GetRequiredService<ILogger<HttpRequest>>();
                    var headers = JsonConvert.SerializeObject(
                        ctx.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
                        Formatting.None);
                    logger.LogInformation("Request headers: {headers}", headers);
                    await next(ctx).ConfigureAwait(false);
                })
            .UseDotAuthServer(x =>
            {
                foreach (var proxy in knownProxies)
                {
                    x.KnownProxies.Add(proxy);
                }
            }, applicationTypes: typeof(IDefaultUi))
            .UseEndpoints(endpoint => { endpoint.MapHealthChecks("/health"); });
    }
}
