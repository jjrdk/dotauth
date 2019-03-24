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
    using SimpleAuth.Shared.Events.Logging;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Linq;
    using System.Net.Http;

    /// <summary>
    /// Defines the service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
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
                "UmaProtection",
                policy =>
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    policy.RequireAuthenticatedUser();
                    //policy.RequireRole("administrator");
                    //policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                    policy.RequireAssertion(
                        p =>
                        {
                            var claimScopes = p.User.Claims.FirstOrDefault(c => c.Type == "scope");
                            return claimScopes != null && claimScopes.Value.Split(' ').Any(s => s == "uma_protection");

                            //return claimRole.Value.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                            //           .Any(role => role == "administrator")
                        });
                });
            options.AddPolicy(
                "authenticated",
                policy =>
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    //policy.AddAuthenticationSchemes(cookieName);
                    policy.RequireAuthenticatedUser();
                });
            options.AddPolicy(
                "manager",
                policy =>
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    //policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                    policy.RequireAssertion(
                        p =>
                        {
                            if (p.User?.Identity?.IsAuthenticated != true)
                            {
                                return false;
                            }

                            var claimsScope = p.User.Claims.Where(c => c.Type == "scope");
                            if (!claimsScope.Any())
                            {
                                return false;
                            }

                            return claimsScope.Any(c => c.Value == "manager");
                        });
                });

            options.AddPolicy(
                "registration",
                policy => // Access token with scope = register_client
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    //policy.AddAuthenticationSchemes("OAuth2Introspection");
                    policy.RequireClaim("scope", "register_client");
                });
            options.AddPolicy(
                "connected_user",
                policy => // Introspect the identity token.
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    //policy.AddAuthenticationSchemes("UserInfoIntrospection");
                    policy.RequireAuthenticatedUser();
                });
            options.AddPolicy(
                "manage_profile",
                policy => // Access token with scope = manage_profile or with role = administrator
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    //policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                    policy.RequireAssertion(
                        p =>
                        {
                            if (p.User?.Identity == null || !p.User.Identity.IsAuthenticated)
                            {
                                return false;
                            }

                            var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == "role");
                            var claimScopes = p.User.Claims.Where(c => c.Type == "scope");
                            if (claimRole == null && !claimScopes.Any())
                            {
                                return false;
                            }

                            return claimRole != null && claimRole.Value == "administrator"
                                   || claimScopes.Any(s => s.Value == "manage_profile");
                        });
                });
            options.AddPolicy(
                "manage_account_filtering",
                policy => // Access token with scope = manage_account_filtering or role = administrator
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    //policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                    policy.RequireAssertion(
                        p =>
                        {
                            if (p.User?.Identity == null || !p.User.Identity.IsAuthenticated)
                            {
                                return false;
                            }

                            var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == "role");
                            var claimScopes = p.User.Claims.Where(c => c.Type == "scope").ToArray();
                            if (claimRole == null && !claimScopes.Any())
                            {
                                return false;
                            }

                            return claimRole != null && claimRole.Value == "administrator"
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
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IAccountFilter, AccountFilter>();
            services.AddSingleton<IFilterStore>(new DefaultFilterStore(filters));
            return services;
        }

        /// <summary>
        /// Adds SimpleAuth.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IServiceCollection AddSimpleAuth(
            this IServiceCollection services,
            Action<SimpleAuthOptions> configuration)
        {
            var options = new SimpleAuthOptions();
            configuration(options);

            return AddSimpleAuth(services, options);
        }

        /// <summary>
        /// Adds the SimpleAuth.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">options</exception>
        public static IServiceCollection AddSimpleAuth(this IServiceCollection services, SimpleAuthOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Globals.ApplicationName = options.ApplicationName ?? string.Empty;
            var runtimeConfig = GetRuntimeConfig(options);
            var s = services.AddTransient<IAuthenticateResourceOwnerService, UsernamePasswordAuthenticationService>()
                .AddTransient<ITwoFactorAuthenticationHandler, TwoFactorAuthenticationHandler>()
                .AddSingleton(runtimeConfig)
                .AddSingleton(options.HttpClientFactory?.Invoke() ?? new HttpClient())
                .AddSingleton(sp => options.EventPublisher?.Invoke(sp) ?? new DefaultEventPublisher())
                .AddSingleton(sp => options.SubjectBuilder?.Invoke(sp) ?? new DefaultSubjectBuilder())
                .AddSingleton(sp => options.JsonWebKeys?.Invoke(sp) ?? new InMemoryJwksRepository())
                .AddSingleton<IJwksStore>(sp => sp.GetService<IJwksRepository>())
                .AddSingleton(
                    sp => options.Clients?.Invoke(sp)
                          ?? new InMemoryClientRepository(sp.GetService<HttpClient>(), sp.GetService<IScopeStore>()))
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
                .AddSingleton(sp => options.AccountFilters?.Invoke(sp) ?? new DefaultFilterStore())
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddTransient<IBasicAuthorizationPolicy, BasicAuthorizationPolicy>();
            services.AddDataProtection();
            return s;
        }

        /// <summary>
        /// Registers the mvc routes for a SimpleAuth server.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSimpleAuthMvc(this IApplicationBuilder app)
        {
            return app.UseMvc(
                routes =>
                {
                    routes.MapRoute("areaexists", "{area:exists}/{controller=Authenticate}/{action=Index}");
                    routes.MapRoute("pwdauth", "pwd/{controller=Authenticate}/{action=Index}");
                    //routes.MapRoute("areaauth", "{area=pwd}/{controller=Authenticate}/{action=Index}");
                    routes.MapRoute("default", "{controller=Authenticate}/{action=Index}");
                });
        }

        private static RuntimeSettings GetRuntimeConfig(SimpleAuthOptions options)
        {
            return new RuntimeSettings(
                onResourceOwnerCreated: options.OnResourceOwnerCreated,
                authorizationCodeValidityPeriod: options.AuthorizationCodeValidityPeriod,
                userClaimsToIncludeInAuthToken: options.UserClaimsToIncludeInAuthToken,
                claimsIncludedInUserCreation: options.ClaimsIncludedInUserCreation,
                rptLifeTime: options.RptLifeTime,
                ticketLifeTime: options.TicketLifeTime);
        }
    }
}
