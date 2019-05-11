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
    using Controllers;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using SimpleAuth;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Repositories;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;

    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly SimpleAuthOptions _options;
        private readonly Assembly _assembly = typeof(HomeController).Assembly;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _options = new SimpleAuthOptions
            {
                ApplicationName = _configuration["ApplicationName"],
                Users = sp => new InMemoryResourceOwnerRepository(DefaultConfiguration.GetUsers()),
                Clients =
                    sp => new InMemoryClientRepository(
                        sp.GetService<HttpClient>(),
                        sp.GetService<IScopeStore>(),
                        DefaultConfiguration.GetClients()),
                Scopes = sp => new InMemoryScopeRepository(DefaultConfiguration.GetScopes()),
                EventPublisher = sp => new ConsolePublisher(),
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
                .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; });
            services.AddAuthentication(CookieNames.ExternalCookieName)
                .AddCookie(CookieNames.ExternalCookieName)
                .AddGoogle(
                    opts =>
                    {
                        opts.AccessType = "offline";
                        opts.ClientId = _configuration["Google:ClientId"];
                        opts.ClientSecret = _configuration["Google:ClientSecret"];
                        opts.SignInScheme = CookieNames.ExternalCookieName;
                        opts.Scope.Add("openid");
                        opts.Scope.Add("profile");
                        opts.Scope.Add("email");
                    })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    {
                        cfg.RequireHttpsMetadata = true;
                        cfg.Authority = _configuration["Endpoint"];
                        cfg.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
                    }); ;
            services.AddAuthorization(opts => { opts.AddAuthPolicies(CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme); })
                .AddMvc(options => { })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddApplicationPart(_assembly);
            services.AddSimpleAuth(_options)
            .AddHttpsRedirection(
                options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                    options.HttpsPort = 5001;
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All })
                .UseHttpsRedirection()
                .UseHsts()
                .UseAuthentication()
                .UseCors("AllowAll")
                .UseStaticFiles(
                    new StaticFileOptions
                    {
                        FileProvider = new EmbeddedFileProvider(_assembly, "SimpleAuth.wwwroot"),
                        OnPrepareResponse = context =>
                        {
                            context.Context.Response.Headers["Cache-Control"] = _configuration["StaticFiles:Headers:Cache-Control"];
                        }
                    })
                .UseSimpleAuthExceptionHandler()
                .UseSimpleAuthExceptionHandler()
                .UseResponseCompression()
                .UseSimpleAuthMvc();
        }
    }
}
