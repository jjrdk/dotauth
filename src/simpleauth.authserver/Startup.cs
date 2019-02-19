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
    using System.IO.Compression;
    using System.Reflection;
    using System.Security.Claims;

    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly SimpleAuthOptions _options;
        private readonly Assembly _assembly = typeof(HomeController).Assembly;

        public Startup()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
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
                UserClaimsToIncludeInAuthToken = new[] { OpenIdClaimTypes.Subject, OpenIdClaimTypes.Role },
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
                    //x.EnableForHttps = true;
                    x.Providers.Add(
                        new GzipCompressionProvider(
                            new GzipCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                    x.Providers.Add(
                        new BrotliCompressionProvider(
                            new BrotliCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                });
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            services.AddLogging(log => { log.AddConsole(); });
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
            services.AddAuthorization(opts => { opts.AddAuthPolicies(CookieNames.CookieName); });
            // 5. Configure MVC
            services.AddMvc(options => { })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddApplicationPart(_assembly);
            services.AddSimpleAuth(_options);
        }

        public void Configure(IApplicationBuilder app)
        {
            //app.UseHttpsRedirection()
            //.UseHsts()
            app.UseAuthentication();
            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 2. Use static files.
            app.UseStaticFiles(
                new StaticFileOptions { FileProvider = new EmbeddedFileProvider(_assembly, "SimpleAuth.wwwroot") });
            app.UseSimpleAuthExceptionHandler();
            // 3. Redirect error to custom pages.
            app.UseStatusCodePagesWithRedirects("/Error/{0}");
            // 4. Enable SimpleAuth
            //app.AddSimpleAuth(_options, loggerFactory);
            // 5. Configure ASP.NET MVC

            app.UseResponseCompression();
            app.UseMvc(
                routes =>
                {
                    routes.MapRoute("pwdauth", "pwd/{controller=Authenticate}/{action=Index}");
                    routes.MapRoute("default", "{controller=Home}/{action=Index}");
                });
        }
    }
}
