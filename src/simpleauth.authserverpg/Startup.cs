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
    using Controllers;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using SimpleAuth;
    using SimpleAuth.Shared.Repositories;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using Marten;
    using Newtonsoft.Json;
    using SimpleAuth.Stores.Marten;

    public class Startup
    {
        private static readonly string DefaultGoogleScopes = "openid,profile,email";
        private readonly IConfiguration _configuration;
        private readonly SimpleAuthOptions _options;
        private readonly Assembly _assembly = typeof(HomeController).Assembly;

        public Startup(IConfiguration configuration)
        {
            var client = new HttpClient();
            _configuration = configuration;
            _options = new SimpleAuthOptions
            {
                ApplicationName = _configuration["ApplicationName"] ?? "SimpleAuth",
                Users = sp => new MartenResourceOwnerStore(sp.GetService<IDocumentSession>),
                Clients =
                    sp => new MartenClientStore(
                        sp.GetService<IDocumentSession>,
                        sp.GetService<IScopeStore>(),
                        sp.GetService<HttpClient>(),
                        JsonConvert.DeserializeObject<Uri[]>),
                Scopes = sp => new MartenScopeRepository(sp.GetService<IDocumentSession>),
                AccountFilters = sp => new MartenFilterStore(sp.GetService<IDocumentSession>),
                AuthorizationCodes = sp => new MartenAuthorizationCodeStore(sp.GetService<IDocumentSession>),
                ConfirmationCodes = sp => new MartenConfirmationCodeStore(sp.GetService<IDocumentSession>),
                Consents = sp => new MartenConsentRepository(sp.GetService<IDocumentSession>),
                HttpClientFactory = () => client,
                JsonWebKeys = sp => new MartenJwksRepository(sp.GetService<IDocumentSession>),
                Policies = sp => new MartenPolicyRepository(sp.GetService<IDocumentSession>),
                Tickets = sp => new MartenTicketStore(sp.GetService<IDocumentSession>),
                Tokens = sp => new MartenTokenStore(sp.GetService<IDocumentSession>),
                ResourceSets = sp => new MartenResourceSetRepository(sp.GetService<IDocumentSession>),
                EventPublisher = sp => new LogEventPublisher(sp.GetService<ILogger<LogEventPublisher>>()),
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
            services.AddSingleton<IDocumentStore>(
                provider =>
                {
                    var options = new SimpleAuthMartenOptions(
                        _configuration["ConnectionString"],
                        new MartenLoggerFacade(provider.GetService<ILogger<MartenLoggerFacade>>()),
                        null,
                        AutoCreate.CreateOrUpdate);
                    return new DocumentStore(options);
                });
            services.AddTransient(sp => sp.GetService<IDocumentStore>().LightweightSession());

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
                .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; });
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
                            foreach (var scope in scopes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
                            {
                                opts.Scope.Add(scope);
                            }
                        });
            }

            services.AddAuthorization(opts => { opts.AddAuthPolicies(CookieNames.CookieName); })
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddApplicationPart(_assembly);
            services.AddRazorPages();
            services.AddSimpleAuth(_options);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSimpleAuthExceptionHandler()
                .UseResponseCompression()
                .UseStaticFiles(
                    new StaticFileOptions
                    {
                        FileProvider = new EmbeddedFileProvider(_assembly, "SimpleAuth.wwwroot")
                    })
                .UseRouting()
                .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All })
                .UseAuthentication()
                .UseAuthorization()
                .UseCors("AllowAll")
                .UseSimpleAuthMvc();
        }
    }
}
