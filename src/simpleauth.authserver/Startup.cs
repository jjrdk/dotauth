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
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using SimpleAuth;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Repositories;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.ResourceServer;
    using SimpleAuth.ResourceServer.Authentication;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Models;

    public class Startup
    {
        private static readonly string DefaultGoogleScopes = "openid,profile,email";
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
                Clients =
                    sp => new InMemoryClientRepository(
                        sp.GetService<HttpClient>(),
                        sp.GetService<IScopeStore>(),
                        sp.GetService<ILogger<InMemoryClientRepository>>(),
                        DefaultConfiguration.GetClients()),
                Scopes = sp => new InMemoryScopeRepository(DefaultConfiguration.GetScopes()),
                ResourceSets =
                    sp => new InMemoryResourceSetRepository(
                        new[] {new ResourceSetModel {Id = "abc", Owner = "user", Scopes = new[] {"read"}}}),
                EventPublisher = sp => new LogEventPublisher(sp.GetService<ILogger<LogEventPublisher>>()),
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
            services.AddUmaClient(new Uri("https://localhost:5001/.well-known/uma2-configuration"));
            services.AddResponseCompression(
                    x =>
                    {
                        x.EnableForHttps = true;
                        x.Providers.Add(
                            new GzipCompressionProvider(
                                new GzipCompressionProviderOptions {Level = CompressionLevel.Optimal}));
                        x.Providers.Add(
                            new BrotliCompressionProvider(
                                new BrotliCompressionProviderOptions {Level = CompressionLevel.Optimal}));
                    })
                .AddHttpContextAccessor()
                .AddAntiforgery(options =>
                {
                    options.FormFieldName = "XrsfField";
                    options.HeaderName = "XSRF-TOKEN";
                    options.SuppressXFrameOptionsHeader = false;
                })
                .AddCors(
                    options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()))
                .AddLogging(log => { log.AddConsole(); });
            services.AddAuthentication(CookieNames.CookieName)
                .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; })
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
                    })
                .AddUmaTicket(configureOptions: options =>
                {
                    options.Authority = "https://localhost:5001/";
                    options.RequireHttpsMetadata = false;
                    options.ClientId = "web";
                    options.ClientSecret = "secret";
                    options.UmaResourcePaths = new[] { new Regex("/x", RegexOptions.Compiled) };
                    options.ResourceSetRequest = r => new[] { new PermissionRequest { ResourceSetId = "abc", Scopes = new[] { "read" } } };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidIssuers = new[] { "http://localhost:5000", "https://localhost:5001" }
                    };
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
                new[] { CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme },
                applicationParts: GetType().Assembly);

            services.AddAuthorization(
                o =>
                {
                    o.AddPolicy("uma_auth", builder => builder.RequireUmaTicket(UmaAuthenticationDefaults.AuthenticationScheme));
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseResponseCompression().UseSimpleAuthMvc();
        }
    }

    [Route("x")]
    public class ResourceController : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "uma_auth")]
        public Task<IActionResult> Get(string resourceId)
        {
            return Task.FromResult<IActionResult>(Ok(resourceId));
        }
    }
}
