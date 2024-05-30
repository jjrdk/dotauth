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

namespace DotAuth.AuthServer;

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using DotAuth;
using DotAuth.Extensions;
using DotAuth.Repositories;
using DotAuth.Shared.Models;
using DotAuth.Shared.Policies;
using DotAuth.Shared.Repositories;
using DotAuth.Sms;
using DotAuth.Sms.Ui;
using DotAuth.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

internal sealed class Startup
{
    private const string DotAuthScheme = "dotauth";
    private readonly IConfiguration _configuration;
    private readonly DotAuthConfiguration _dotAuthConfiguration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
        _ = bool.TryParse(_configuration["REDIRECT"], out var redirect);
        var salt = _configuration["SALT"] ?? string.Empty;
        _dotAuthConfiguration = new DotAuthConfiguration(salt)
        {
            AllowHttp = true,
            RedirectToLogin = redirect,
            DataProtector = _ => new SymmetricDataProtector(Aes.Create()),
            ApplicationName = _configuration["SERVER_NAME"] ?? "DotAuth",
            Users = _ => new InMemoryResourceOwnerRepository(salt, DefaultConfiguration.GetUsers(salt)),
            Tickets = _ => new InMemoryTicketStore(),
            Clients =
                sp => new InMemoryClientRepository(
                    sp.GetRequiredService<IHttpClientFactory>(),
                    sp.GetRequiredService<IScopeStore>(),
                    sp.GetRequiredService<ILogger<InMemoryClientRepository>>(),
                    DefaultConfiguration.GetClients()),
            Scopes = _ => new InMemoryScopeRepository(DefaultConfiguration.GetScopes()),
            ResourceSets =
                sp => new InMemoryResourceSetRepository(
                    sp.GetRequiredService<IAuthorizationPolicy>(),
                    new[]
                    {
                        ("administrator",
                         new ResourceSet
                         {
                             Id = "abc",
                             Name = "Test Resource",
                             Type = "Content",
                             Scopes = ["read"],
                             AuthorizationPolicies =
                             [
                                 new PolicyRule
                                 {
                                     Claims =
                                     [
                                         new ClaimData
                                         {
                                             Type = "sub", Value = "administrator"
                                         }
                                     ],
                                     ClientIdsAllowed = ["web"],
                                     Scopes = ["read"],
                                     IsResourceOwnerConsentNeeded = true
                                 }
                             ]
                         })
                    }),
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
        services.AddHttpClient()
            .AddLogging(log =>
            {
                log.AddJsonConsole(
                    o =>
                    {
                        o.IncludeScopes = true;
                        o.UseUtcTimestamp = true;
                        o.JsonWriterOptions = new JsonWriterOptions { Indented = false };
                    });
            })
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
                    cfg.RequireHttpsMetadata = false;
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
                        opts.ClientId = _configuration["GOOGLE:CLIENTID"] ?? "";
                        opts.ClientSecret = _configuration["GOOGLE:CLIENTSECRET"] ?? "";
                        opts.SignInScheme = CookieNames.ExternalCookieName;
                        var scopes = _configuration["GOOGLE:SCOPES"] ?? "openid,profile,email";
                        foreach (var scope in scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim()))
                        {
                            opts.Scope.Add(scope);
                        }
                    });
        }

        if (!string.IsNullOrWhiteSpace(_configuration["MS:CLIENTID"])
         && !string.IsNullOrWhiteSpace(_configuration["MS:CLIENTSECRET"]))
        {
            services.AddAuthentication(CookieNames.ExternalCookieName).AddMicrosoftAccount(
                opts =>
                {
                    opts.ClientId = _configuration["MS:CLIENTID"]!;
                    opts.ClientSecret = _configuration["MS:CLIENTSECRET"]!;
                    opts.UsePkce = true;
                    var scopes = _configuration["MS:SCOPES"] ?? "openid,profile,email";
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
        var pathBase = _configuration["PATHBASE"];
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            app = app.UsePathBase(pathBase);
        }

        app.UseResponseCompression()
            .UseDotAuthServer(applicationTypes: typeof(IDefaultUi));
    }
}
