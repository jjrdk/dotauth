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

namespace SimpleIdentityServer.Host.Extensions
{
    using Core;
    using Core.Jwt;
    using Logging;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Parsers;
    using System;
    using System.Linq;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenIdApi(this IServiceCollection serviceCollection, Action<IdentityServerOptions> optionsCallback)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (optionsCallback == null)
            {
                throw new ArgumentNullException(nameof(optionsCallback));
            }

            var options = new IdentityServerOptions();
            optionsCallback(options);
            serviceCollection.AddOpenIdApi(options);
            return serviceCollection;
        }

        /// <summary>
        /// Add the OPENID API.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenIdApi(this IServiceCollection serviceCollection, IdentityServerOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            ConfigureSimpleIdentityServer(
                serviceCollection,
                options);
            return serviceCollection;
        }

        public static AuthorizationOptions AddOpenIdSecurityPolicy(this AuthorizationOptions authenticateOptions, string cookieName)
        {
            if (authenticateOptions == null)
            {
                throw new ArgumentNullException(nameof(authenticateOptions));
            }

            authenticateOptions.AddPolicy("Connected", policy => // User is connected
            {
                policy.AddAuthenticationSchemes(cookieName);
                policy.RequireAuthenticatedUser();
            });
            authenticateOptions.AddPolicy("registration", policy => // Access token with scope = register_client
            {
                policy.AddAuthenticationSchemes("OAuth2Introspection");
                policy.RequireClaim("scope", "register_client");
            });
            authenticateOptions.AddPolicy("connected_user", policy => // Introspect the identity token.
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection");
                policy.RequireAuthenticatedUser();
            });
            authenticateOptions.AddPolicy("manage_profile", policy => // Access token with scope = manage_profile or with role = administrator
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
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

                    return claimRole != null && claimRole.Value == "administrator" || claimScopes.Any(s => s.Value == "manage_profile");
                });
            });
            authenticateOptions.AddPolicy("manage_account_filtering", policy => // Access token with scope = manage_account_filtering or role = administrator
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
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

                    return claimRole != null && claimRole.Value == "administrator" || claimScopes.Any(s => s.Value == "manage_account_filtering");
                });
            });
            return authenticateOptions;
        }

        private static void ConfigureSimpleIdentityServer(
            IServiceCollection services,
            IdentityServerOptions options)
        {
            services.AddSimpleIdentityServerCore(
                    options.OAuthConfigurationOptions,
                    clients: options.Configuration?.Clients,
                    resourceOwners: options.Configuration?.Users,
                    translations: options.Configuration?.Translations,
                    jsonWebKeys: options.Configuration?.JsonWebKeys)
                .AddSimpleIdentityServerJwt()
                .AddHostIdentityServer(options)
                //.AddIdServerClient()
                .AddDefaultTokenStore()
                //.AddDefaultAccessTokenStore()
                //.AddDefaultSimpleBus()
                .AddTechnicalLogging()
                .AddOpenidLogging()
                .AddOAuthLogging()
                .AddDataProtection();
        }

        public static IServiceCollection AddHostIdentityServer(this IServiceCollection serviceCollection, IdentityServerOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            serviceCollection
                .AddSingleton(options.Scim)
                .AddTransient<IRedirectInstructionParser, RedirectInstructionParser>()
                .AddTransient<IActionResultParser, ActionResultParser>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddDataProtection();
            return serviceCollection;
        }
    }
}