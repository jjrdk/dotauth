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
    using System.IdentityModel.Tokens.Jwt;
    using Extensions;

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
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using SimpleAuth.ResourceServer;
    using SimpleAuth.ResourceServer.Authentication;
    using SimpleAuth.Shared.Models;

    internal class Startup
    {
        private const string SimpleAuthScheme = "simpleauth";
        private readonly IConfiguration _configuration;
        private readonly SimpleAuthOptions _options;

        public Startup(IConfiguration configuration)
        {
            var client = new HttpClient();
            _configuration = configuration;
            _options = new SimpleAuthOptions
            {
                ApplicationName = _configuration["ApplicationName"] ?? "SimpleAuth",
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
                HttpClientFactory = () => client,
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
            services.AddHttpContextAccessor()
                .AddAntiforgery(
                    options =>
                    {
                        options.FormFieldName = "XrsfField";
                        options.HeaderName = "XSRF-TOKEN";
                        options.SuppressXFrameOptionsHeader = false;
                    })
                .AddCors(
                    options => options.AddPolicy(
                        "AllowAll",
                        p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders()))
                .AddLogging(log => { log.AddConsole(); });
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieNames.CookieName;
                options.DefaultChallengeScheme = SimpleAuthScheme;
            })
                .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; })
                .AddOAuth(
                    SimpleAuthScheme,
                    '_' + SimpleAuthScheme,
                    options =>
                    {
                    })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    {
                        cfg.Authority = "https://localhost:5001";
                        cfg.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidIssuers = new[] { "http://localhost:5000", "https://localhost:5001" }
                        };
                        cfg.RequireHttpsMetadata = false;
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

            services.AddSimpleAuth(
                _options,
                new[] { CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, SimpleAuthScheme },
                applicationParts: GetType().Assembly);

            services.AddAuthorization(
                o =>
                {
                    o.AddPolicy(
                        "uma_auth",
                        builder => builder.RequireUmaTicket(UmaAuthenticationDefaults.AuthenticationScheme));
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseResponseCompression().UseSimpleAuthMvc();
        }
    }

    internal class ConfigureOAuthOptions : IPostConfigureOptions<OAuthOptions>
    {
        /// <inheritdoc />
        public void PostConfigure(string name, OAuthOptions options)
        {
            options.AuthorizationEndpoint = "https://localhost:5001/authorization";
            options.TokenEndpoint = "https://localhost:5001/token";
            options.UserInformationEndpoint = "https://localhost:5001/userinfo";
            options.UsePkce = true;
            options.CallbackPath = "/callback";
            options.Events = new OAuthEvents
            {
                OnCreatingTicket = ctx =>
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(ctx.AccessToken);
                    ctx.Identity.AddClaims(jwt.Claims.Where(c => !ctx.Identity.HasClaim(x => x.Type == c.Type)));
                    ctx.Success();
                    return Task.CompletedTask;
                },
                OnTicketReceived = ctx => Task.CompletedTask
            };
            options.SaveTokens = true;
            //#if DEBUG
            //options.RequireHttpsMetadata = false;
            //#endif
            //options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
            //options.DisableTelemetry = true;
            options.ClientId = "web";
            options.ClientSecret = "secret";
            //options.ResponseType = OpenIdConnectResponseType.Code;
            //options.ResponseMode = OpenIdConnectResponseMode.FormPost;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("uma_protection");
        }
    }
}
