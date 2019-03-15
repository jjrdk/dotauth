namespace SimpleAuth.AwsLambda
{
    using Controllers;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Shared.Repositories;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Loader;
    using System.Security.Claims;
    using SimpleAuth.Repositories;

    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly SimpleAuthOptions _options;
        private readonly Assembly _assembly = typeof(HomeController).Assembly;
        private readonly Assembly _views;

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            var path = Path.GetFullPath("simpleauth.Views.dll");
            _views = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            _configuration = new ConfigurationBuilder().SetBasePath(hostingEnvironment.ContentRootPath).Build();
            _options = new SimpleAuthOptions
            {
                ApplicationName = "iThemba",
                Users = sp => new InMemoryResourceOwnerRepository(DefaultConfiguration.GetUsers()),
                Clients =
                    sp => new InMemoryClientRepository(
                        null,
                        sp.GetService<IScopeStore>(),
                        DefaultConfiguration.GetClients()),
                UserClaimsToIncludeInAuthToken = new[] { "sub", "role" },
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
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            services.AddLogging();
            services.AddAuthentication(CookieNames.CookieName)
                .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; });

            services.AddAuthentication(CookieNames.ExternalCookieName)
                .AddCookie(CookieNames.ExternalCookieName)
                .AddGoogle(
                    opts =>
                    {
                        opts.AccessType = "offline";
                        opts.ClientId = "id";
                        opts.ClientSecret = "secret";
                        opts.SignInScheme = CookieNames.ExternalCookieName;
                        opts.Scope.Add("openid");
                        opts.Scope.Add("profile");
                        opts.Scope.Add("email");
                    });
            services.AddAuthorization(opts => { opts.AddAuthPolicies(CookieNames.CookieName); });
            // 5. Configure MVC
            services.AddMvc(options => { }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .ConfigureApplicationPartManager(
                    _ =>
                    {
                        _.ApplicationParts.Add(new AssemblyPart(_assembly));
                        _.ApplicationParts.Add(new CompiledRazorAssemblyPart(_views));
                    });
            services.AddSimpleAuth(_options);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLambdaLogger(_configuration).AddConsole();
            app.UseAuthentication();

            // Enable CORS.
            app.UseCors("AllowAll");
            // Use static files.
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new EmbeddedFileProvider(_assembly, "SimpleAuth.wwwroot")
                });
            // Redirect error to custom pages.
            app.UseStatusCodePagesWithRedirects("~/Error/{0}");

            // Configure ASP.NET MVC

            app.UseMvc(routes => { routes.MapRoute("default", "{controller=Home}/{action=Index}"); });
        }
    }
}
