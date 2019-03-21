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
    using Amazon;
    using Amazon.Runtime;
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
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Sms;
    using System.IO.Compression;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text.RegularExpressions;

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
                        null,
                        sp.GetService<IScopeStore>(),
                        DefaultConfiguration.GetClients()),
                Scopes = sp => new InMemoryScopeRepository(),
                EventPublisher = sp => new ConsolePublisher(),
                UserClaimsToIncludeInAuthToken =
                    new[]
                    {
                        new Regex($"^{OpenIdClaimTypes.Subject}$", RegexOptions.Compiled),
                        new Regex($"^{OpenIdClaimTypes.Role}$", RegexOptions.Compiled)
                    },
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
                                new GzipCompressionProviderOptions {Level = CompressionLevel.Optimal}));
                        x.Providers.Add(
                            new BrotliCompressionProvider(
                                new BrotliCompressionProviderOptions {Level = CompressionLevel.Optimal}));
                    })
                .AddHttpContextAccessor()
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
                    });
            services.AddAuthorization(opts => { opts.AddAuthPolicies(CookieNames.CookieName); })
                .AddMvc(options => { })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddApplicationPart(_assembly)
                .AddSmsAuthentication(
                    new AwsSmsClient(
                        new BasicAWSCredentials(_configuration["Aws:AccessKey"], _configuration["Aws:Secret"]),
                        RegionEndpoint.EUWest1,
                        _configuration["ApplicationName"]));
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
            app.UseForwardedHeaders(new ForwardedHeadersOptions {ForwardedHeaders = ForwardedHeaders.All})
                .UseHttpsRedirection()
                .UseHsts()
                .UseAuthentication()
                .UseCors("AllowAll")
                .UseStaticFiles(
                    new StaticFileOptions {FileProvider = new EmbeddedFileProvider(_assembly, "SimpleAuth.wwwroot")})
                .UseSimpleAuthExceptionHandler()
                //.UseStatusCodePagesWithRedirects("/Error/{0}")
                .UseResponseCompression()
                .UseMvc(
                    routes =>
                    {
                        routes.MapRoute("areaexists", "{area:exists}/{controller=Authenticate}/{action=Index}");
                        routes.MapRoute("pwdauth", "pwd/{controller=Authenticate}/{action=Index}");
                        //routes.MapRoute("areaauth", "{area=pwd}/{controller=Authenticate}/{action=Index}");
                        routes.MapRoute("default", "{controller=Authenticate}/{action=Index}");
                    });
        }
    }
}
