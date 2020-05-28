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

namespace SimpleAuth.AuthServer
{
    using System;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;

    using SimpleAuth;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Repositories;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using Amazon;
    using Amazon.Runtime;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Sms;
    using SimpleAuth.Sms.Ui;
    using SimpleAuth.UI;

    internal class Startup
    {
        private const string SimpleAuthScheme = "simpleauth";
        private readonly IConfiguration _configuration;
        private readonly SimpleAuthOptions _options;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            bool.TryParse(_configuration["REDIRECT"], out var redirect);
            _options = new SimpleAuthOptions
            {
                RedirectToLogin = redirect,
                ApplicationName = _configuration["SERVER_NAME"] ?? "SimpleAuth",
                Users = sp => new InMemoryResourceOwnerRepository(DefaultConfiguration.GetUsers()),
                Tickets = sp => new InMemoryTicketStore(),
                Clients =
                    sp => new InMemoryClientRepository(
                        sp.GetRequiredService<HttpClient>(),
                        sp.GetRequiredService<IScopeStore>(),
                        sp.GetRequiredService<ILogger<InMemoryClientRepository>>(),
                        DefaultConfiguration.GetClients()),
                Scopes = sp => new InMemoryScopeRepository(DefaultConfiguration.GetScopes()),
                ResourceSets =
                    sp => new InMemoryResourceSetRepository(
                        new[]
                        {
                            ("administrator",
                                new ResourceSet
                                {
                                    Id = "abc",
                                    Name = "Test Resource",
                                    Type = "Content",
                                    Scopes = new[] {"read"},
                                    AuthorizationPolicies = new[]
                                    {
                                        new PolicyRule
                                        {
                                            Claims = new[]
                                            {
                                                new ClaimData
                                                {
                                                    Type = "sub", Value = "administrator"
                                                }
                                            },
                                            ClientIdsAllowed = new[] {"web"},
                                            Scopes = new[] {"read"},
                                            IsResourceOwnerConsentNeeded = true
                                        }
                                    }
                                })
                        }),
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
            services.AddSingleton<HttpClient>()
                .AddLogging(log => { log.AddConsole(); });
            services.AddAuthentication(
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
                        cfg.Authority = _configuration["OAUTH_AUTHORITY"];
                        cfg.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidIssuers = _configuration["OAUTH_VALID_ISSUERS"]
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .ToArray()
                        };
#if DEBUG
                        cfg.RequireHttpsMetadata = false;
#endif
                    });
            services.ConfigureOptions<ConfigureOAuthOptions>();

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
                            var scopes = _configuration["Google:Scopes"] ?? "openid,profile,email";
                            foreach (var scope in scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim()))
                            {
                                opts.Scope.Add(scope);
                            }
                        });
            }

            if (!string.IsNullOrWhiteSpace(_configuration["Amazon:AccessKey"])
                && !string.IsNullOrWhiteSpace(_configuration["Amazon:SecretKey"]))
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
                                _configuration["Amazon:AccessKey"],
                                _configuration["Amazon:SecretKey"]),
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
            app.UseResponseCompression()
                .UseSimpleAuthMvc(
                    (typeof(IDefaultUi).Namespace, typeof(IDefaultUi).Assembly));
        }
    }
}
