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

namespace SimpleAuth.Extensions
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleAuth.Policies;
    using SimpleAuth.Repositories;
    using SimpleAuth.Services;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.MiddleWare;
    using SimpleAuth.Shared.Events;

    /// <summary>
    /// Defines the service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private const string ManageProfileClaim = "manage_profile";
        private const string AdministratorRole = "administrator";
        private const string RoleType = "role";
        private const string ScopeType = "scope";

        /// <summary>
        /// Adds the authentication policies.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="authenticationSchemes">The authentication schemes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">options</exception>
        public static AuthorizationOptions AddAuthPolicies(
            this AuthorizationOptions options,
            params string[] authenticationSchemes)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            options.AddPolicy(
                "authenticated",
                policy =>
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    policy.RequireAuthenticatedUser();
                });
            options.AddPolicy(
                "UmaProtection",
                policy =>
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(
                        p =>
                        {
                            if (p.User?.Identity?.IsAuthenticated != true)
                            {
                                return false;
                            }

                            var claimScopes = p.User?.Claims?.FirstOrDefault(c => c.Type == ScopeType);
                            return claimScopes != null && claimScopes.Value.Split(' ').Any(s => s == "uma_protection");
                        });
                });
            options.AddPolicy(
                "manager",
                policy =>
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(
                        p =>
                        {
                            if (p.User?.Identity?.IsAuthenticated != true)
                            {
                                return false;
                            }

                            var result = p.User?.Claims?.Where(c => c.Type == ScopeType).Any(c => c.HasClaimValue("manager"));

                            return result == true;
                        });
                });

            options.AddPolicy(
                "registration",
                policy => // Access token with scope = register_client
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(
                        p =>
                        {
                            if (p.User?.Identity?.IsAuthenticated != true)
                            {
                                return false;
                            }

                            var claimsScopes = p.User.Claims.Where(c => c.Type == ScopeType);

                            return claimsScopes.SelectMany(c => c.Value.Split(' ')).Any(v => v == "register_client");
                        });
                    policy.RequireAssertion(
                        p =>
                        {
                            if (p.User?.Identity?.IsAuthenticated != true)
                            {
                                return false;
                            }

                            var result = p.User?.Claims?.Where(c => c.Type == ScopeType).Any(c => c.Value == "register_client");

                            return result == true;
                        });
                });
            options.AddPolicy(
                ManageProfileClaim,
                policy => // Access token with scope = manage_profile or with role = administrator
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(
                        p =>
                        {
                            if (p.User?.Identity?.IsAuthenticated != true)
                            {
                                return false;
                            }

                            var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == RoleType);
                            var claimScopes = p.User.Claims.Where(c => c.Type == ScopeType).ToArray();
                            if (claimRole == null && claimScopes.Length == 0)
                            {
                                return false;
                            }

                            return claimRole != null && claimRole.Value == AdministratorRole
                                   || claimScopes.Any(s => s.Value == ManageProfileClaim);
                        });
                });
            options.AddPolicy(
                "manage_account_filtering",
                policy => // Access token with scope = manage_account_filtering or role = administrator
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(
                        p =>
                        {
                            if (p.User?.Identity == null || !p.User.Identity.IsAuthenticated)
                            {
                                return false;
                            }

                            var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == RoleType);
                            var claimScopes = p.User.Claims.Where(c => c.Type == ScopeType).ToArray();
                            if (claimRole == null && !claimScopes.Any())
                            {
                                return false;
                            }

                            return claimRole != null && claimRole.Value.Split(' ', ',').Any(v => v == AdministratorRole)
                                   || claimScopes.SelectMany(s => s.Value.Split(' '))
                                       .Any(s => s == "manage_account_filtering");
                        });
                });
            return options;
        }

        /// <summary>
        /// Adds the account filter.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">services</exception>
        public static IServiceCollection AddAccountFilter(this IServiceCollection services, params Filter[] filters)
        {
            services.AddTransient<IAccountFilter, AccountFilter>();
            services.AddSingleton<IFilterStore>(new InMemoryFilterStore(filters));
            return services;
        }

        /// <summary>
        /// Adds SimpleAuth type registrations.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="requestThrottle">The rate limiter.</param>
        /// <param name="authPolicies"></param>
        /// <returns>An <see cref="IMvcBuilder"/> instance.</returns>
        public static IMvcBuilder AddSimpleAuth(
            this IServiceCollection services,
            Action<SimpleAuthOptions> configuration,
            string[] authPolicies,
            IRequestThrottle requestThrottle = null)
        {
            var options = new SimpleAuthOptions();
            configuration(options);

            return AddSimpleAuth(services, options, authPolicies, requestThrottle);
        }

        /// <summary>
        /// Adds SimpleAuth type registrations.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">The options.</param>
        /// <param name="requestThrottle">The rate limiter.</param>
        /// <param name="authPolicies"></param>
        /// <param name="applicationParts">Assemblies with additional application parts.</param>
        /// <returns>An <see cref="IMvcBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">options</exception>
        public static IMvcBuilder AddSimpleAuth(
            this IServiceCollection services,
            SimpleAuthOptions options,
            string[] authPolicies,
            IRequestThrottle requestThrottle = null,
            params Assembly[] applicationParts)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var mvcBuilder = services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);
            mvcBuilder = applicationParts.Concat(new[] { typeof(ServiceCollectionExtensions).Assembly })
                .Distinct()
                .Aggregate(mvcBuilder, (b, a) => b.AddApplicationPart(a));
            services.AddRazorPages();
            Globals.ApplicationName = options.ApplicationName ?? "SimpleAuth";
            var runtimeConfig = GetRuntimeConfig(options);
            services.AddAuthentication();
            var policies = authPolicies?.Length > 0 ? authPolicies : new[] { CookieNames.CookieName };
            services.AddAuthorization(opts => { opts.AddAuthPolicies(policies); });

            var s = services.AddTransient<IAuthenticateResourceOwnerService, UsernamePasswordAuthenticationService>()
                .AddTransient<ITwoFactorAuthenticationHandler, TwoFactorAuthenticationHandler>()
                .ConfigureOptions<ConfigureMvcNewtonsoftJsonOptions>()
                .AddSingleton(runtimeConfig)
                .AddSingleton(requestThrottle ?? NoopThrottle.Default)
                .AddSingleton(sp => options.HttpClientFactory.Invoke())
                .AddSingleton(sp => options.EventPublisher?.Invoke(sp) ?? new NoopEventPublisher())
                .AddSingleton(sp => options.SubjectBuilder?.Invoke(sp) ?? new DefaultSubjectBuilder())
                .AddSingleton(sp => options.JsonWebKeys?.Invoke(sp) ?? new InMemoryJwksRepository())
                .AddSingleton<IJwksStore>(sp => sp.GetService<IJwksRepository>())
                .AddSingleton(
                    sp => options.Clients?.Invoke(sp)
                          ?? new InMemoryClientRepository(
                              sp.GetService<HttpClient>(),
                              sp.GetService<IScopeStore>(),
                              sp.GetService<ILogger<InMemoryClientRepository>>()))
                .AddSingleton<IClientStore>(sp => sp.GetService<IClientRepository>())
                .AddSingleton(sp => options.Consents?.Invoke(sp) ?? new InMemoryConsentRepository())
                .AddSingleton<IConsentStore>(sp => sp.GetService<IConsentRepository>())
                .AddSingleton(sp => options.Users?.Invoke(sp) ?? new InMemoryResourceOwnerRepository())
                .AddSingleton<IResourceOwnerStore>(sp => sp.GetService<IResourceOwnerRepository>())
                .AddSingleton(sp => options.Scopes?.Invoke(sp) ?? new InMemoryScopeRepository())
                .AddSingleton<IScopeStore>(sp => sp.GetService<IScopeRepository>())
                .AddSingleton(sp => options.Policies?.Invoke(sp) ?? new InMemoryPolicyRepository())
                .AddSingleton(sp => options.ResourceSets?.Invoke(sp) ?? new InMemoryResourceSetRepository())
                .AddSingleton(sp => options.Tickets?.Invoke(sp) ?? new InMemoryTicketStore())
                .AddSingleton(sp => options.AuthorizationCodes?.Invoke(sp) ?? new InMemoryAuthorizationCodeStore())
                .AddSingleton(sp => options.Tokens?.Invoke(sp) ?? new InMemoryTokenStore())
                .AddSingleton(sp => options.ConfirmationCodes?.Invoke(sp) ?? new InMemoryConfirmationCodeStore())
                .AddSingleton(sp => options.AccountFilters?.Invoke(sp) ?? new InMemoryFilterStore())
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddTransient<IAuthorizationPolicy, DefaultAuthorizationPolicy>();
            s.AddDataProtection();
            return mvcBuilder;
        }

        /// <summary>
        /// Registers the mvc routes for a SimpleAuth server.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSimpleAuthMvc(this IApplicationBuilder app)
        {
            var publisher = app.ApplicationServices.GetService(typeof(IEventPublisher)) ?? new NoOpPublisher();
            return app.UseMiddleware<ExceptionHandlerMiddleware>(publisher)
                //.UseResponseCompression()
                .UseStaticFiles(
                    new StaticFileOptions
                    {
                        FileProvider = new EmbeddedFileProvider(
                            typeof(ServiceCollectionExtensions).Assembly,
                            "SimpleAuth.wwwroot")
                    })
                .UseRouting()
                .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All })
                .UseAuthentication()
                .UseAuthorization()
                .UseCors("AllowAll")
                .UseEndpoints(
                    endpoint =>
                    {
                        endpoint.MapRazorPages();
                        endpoint.MapControllerRoute(
                            "areaexists",
                            "{area:exists}/{controller=Authenticate}/{action=Index}");
                        endpoint.MapControllerRoute("pwdauth", "pwd/{controller=Authenticate}/{action=Index}");
                        endpoint.MapControllerRoute("default", "{controller=Authenticate}/{action=Index}");
                    });
        }

        private static RuntimeSettings GetRuntimeConfig(SimpleAuthOptions options)
        {
            return new RuntimeSettings(
                onResourceOwnerCreated: options.OnResourceOwnerCreated,
                authorizationCodeValidityPeriod: options.AuthorizationCodeValidityPeriod,
                claimsIncludedInUserCreation: options.ClaimsIncludedInUserCreation,
                rptLifeTime: options.RptLifeTime,
                ticketLifeTime: options.TicketLifeTime);
        }
    }
}
