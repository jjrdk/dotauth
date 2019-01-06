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
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Server;
    using Server.Controllers;
    using Server.Extensions;
    using SimpleAuth;
    using System;
    using System.Reflection;
    using Microsoft.AspNetCore.Http;

    public class Startup
    {
        private readonly SimpleAuthOptions _options;
        private readonly Assembly _assembly = typeof(HomeController).Assembly;

        public Startup()
        {
            _options = new SimpleAuthOptions
            {
                Configuration = new OpenIdServerConfiguration
                {
                    Users = DefaultConfiguration.GetUsers(),
                    // JsonWebKeys = DefaultConfiguration.GetJsonWebKeys(),
                    Clients = DefaultConfiguration.GetClients()
                },
                Scim = new ScimOptions
                {
                    IsEnabled = false
                }
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            services.AddLogging();
            services.AddAuthentication(HostConstants.CookieNames.CookieName)
                .AddCookie(HostConstants.CookieNames.CookieName, opts =>
                {
                    opts.LoginPath = "/Authenticate";
                });
            services.AddAuthorization(opts =>
            {
                opts.AddAuthPolicies(HostConstants.CookieNames.CookieName);
            });
            // 5. Configure MVC
            var emb = _assembly.GetManifestResourceNames();
            Console.WriteLine(emb.Length);
            var mvcBuilder = services.AddMvc(
                    options => { })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddApplicationPart(_assembly);
            services.UseSimpleAuth(_options);
            services.AddDefaultTokenStore();
            services.Configure<RazorViewEngineOptions>(x =>
            {
                x.FileProviders.Add(new EmbeddedFileProvider(_assembly, "SimpleAuth.Server"));
            });

            // API
            //services.AddBasicShell(mvcBuilder);  // SHELL
            //services.AddLoginPasswordAuthentication(mvcBuilder, new BasicAuthenticateOptions());  // LOGIN & PASSWORD
            //services.AddUserManagement(mvcBuilder);  // USER MANAGEMENT
        }

        public void Configure(
            IApplicationBuilder app,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseAuthentication();
            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 2. Use static files.
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(_assembly, "SimpleAuth.Server.wwwroot")
            });
            app.UseSimpleAuth(_options);
            // 3. Redirect error to custom pages.
            app.UseStatusCodePagesWithRedirects("~/Error/{0}");
            // 4. Enable SimpleAuth
            //app.UseSimpleAuth(_options, loggerFactory);
            // 5. Configure ASP.NET MVC
            app.UseMvc(routes =>
            {
                //routes.UseLoginPasswordAuthentication();
                routes.MapRoute("AuthArea",
                    "{area:exists}/Authenticate/{action}/{id?}",
                    new { controller = "Authenticate", action = "Index" });
                routes.MapRoute("default", "{controller=Home}/{action=Index}");
                //routes.UseUserManagement();
                //routes.UseShell();
            });
        }
    }
}