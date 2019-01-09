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
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.CodeAnalysis;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Server;
    using Server.Controllers;
    using Server.Extensions;
    using SimpleAuth;
    using System;
    using System.IO.Compression;
    using System.Reflection;

    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly SimpleAuthOptions _options;
        private readonly Assembly _assembly = typeof(HomeController).Assembly;

        public Startup()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            _options = new SimpleAuthOptions
            {
                Configuration = new OpenIdServerConfiguration
                {
                    Users = DefaultConfiguration.GetUsers(),
                    Translations = DefaultConfiguration.GetTranslations(),
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
            services.AddResponseCompression(x =>
            {
                x.EnableForHttps = true;
                x.Providers.Add(
                    new GzipCompressionProvider(
                        new GzipCompressionProviderOptions
                        { Level = CompressionLevel.Optimal }));
                x.Providers.Add(
                    new BrotliCompressionProvider(
                        new BrotliCompressionProviderOptions
                        { Level = CompressionLevel.Optimal }));
            });
            services.AddHttpsRedirection(x =>
            {
                x.HttpsPort = 443;
            });
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddCors(options => options.AddPolicy(
                "AllowAll",
                p => p.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()));
            services.AddLogging();
            services.AddAuthentication(HostConstants.CookieNames.CookieName)
                .AddCookie(HostConstants.CookieNames.CookieName,
                    opts => { opts.LoginPath = "/Authenticate"; });

            services.AddAuthentication(HostConstants.CookieNames.ExternalCookieName)
                .AddCookie(HostConstants.CookieNames.ExternalCookieName)
                .AddGoogle(opts =>
                {
                    opts.AccessType = "offline";
                    opts.ClientId = _configuration["Google:ClientId"];
                    opts.ClientSecret = _configuration["Google:ClientSecret"];
                    opts.SignInScheme = HostConstants.CookieNames.ExternalCookieName;
                    opts.Scope.Add("openid");
                    opts.Scope.Add("profile");
                    opts.Scope.Add("email");
                });
            services.AddAuthorization(opts =>
            {
                opts.AddAuthPolicies(HostConstants.CookieNames.CookieName);
            });
            // 5. Configure MVC
            var emb = _assembly.GetManifestResourceNames();
            Console.WriteLine(emb.Length);
            var mvcBuilder = services.AddMvc(
                    options =>
                    {
                        // options.InputFormatters.Add(new );
                    })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddApplicationPart(_assembly);
            services.AddSimpleAuth(_options);
            services.AddDefaultTokenStore();
            services.Configure<RazorViewEngineOptions>(x =>
            {
                x.FileProviders.Add(new EmbeddedFileProvider(_assembly, "SimpleAuth.Server"));
                x.AdditionalCompilationReferences.Add(MetadataReference.CreateFromFile(typeof(BasicAuthenticateOptions).Assembly.Location));
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

            app.UseHttpsRedirection()
                //.UseHsts()
                .UseAuthentication();
            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 2. Use static files.
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(_assembly, "SimpleAuth.Server.wwwroot")
            });
            app.UseSimpleAuth();
            // 3. Redirect error to custom pages.
            app.UseStatusCodePagesWithRedirects("~/Error/{0}");
            // 4. Enable SimpleAuth
            //app.AddSimpleAuth(_options, loggerFactory);
            // 5. Configure ASP.NET MVC

            app.UseResponseCompression();
            app.UseMvc(routes =>
            {
                //routes.UseLoginPasswordAuthentication();
                //routes.MapRoute("AuthArea",
                //    "{area:exists}/Authenticate/{action}/{id?}",
                //    new { controller = "Authenticate", action = "Index" });
                routes.MapRoute("default", "{controller=Home}/{action=Index}");
                //routes.UseUserManagement();
                //routes.UseShell();
            });
        }
    }
}