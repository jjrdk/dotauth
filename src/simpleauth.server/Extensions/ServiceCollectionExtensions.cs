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

namespace SimpleAuth.Server.Extensions
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Parsers;
    using Shared;
    using Shared.AccountFiltering;
    using Shared.Repositories;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Logging;

    public static class ServiceCollectionExtensions
    {
        //public static IServiceCollection AddScimHost(this IServiceCollection services)
        //{
        //    if (services == null)
        //    {
        //        throw new ArgumentNullException(nameof(services));
        //    }

        //    services.AddTransient<IParametersValidator, ParametersValidator>();
        //    services.AddSingleton(new InMemoryGroupsRepository());
        //    services.AddSingleton<IProvide<GroupResource>>(sp => sp.GetService<InMemoryGroupsRepository>());
        //    services.AddSingleton<IPersist<GroupResource>>(sp => sp.GetService<InMemoryGroupsRepository>());
        //    services.AddSingleton<IStore<GroupResource>>(sp => sp.GetService<InMemoryGroupsRepository>());
        //    return services;
        //}

        public static AuthorizationOptions AddAuthPolicies(this AuthorizationOptions options, string cookieName)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.AddPolicy(ScimConstants.ScimPolicies.ScimManage, policy =>
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
                {
                    if (p.User?.Identity?.IsAuthenticated != true)
                    {
                        return false;
                    }

                    var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == "role");
                    var claimScopes = p.User.Claims.Where(c => c.Type == "scope");
                    if (claimRole == null && !claimScopes.Any())
                    {
                        return false;
                    }

                    return claimRole != null && claimRole.Value == "administrator" || claimScopes.Any(c => c.Value == ScimConstants.ScimPolicies.ScimManage);
                });
            });
            options.AddPolicy("scim_read", policy =>
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

                    return claimRole != null && claimRole.Value == "administrator" || claimScopes.Any(c => c.Value == "scim_read");
                });
            });
            options.AddPolicy("authenticated", policy =>
            {
                //policy.AddAuthenticationSchemes("UserInfoIntrospection");
                policy.RequireAuthenticatedUser();
            });
            options.AddPolicy("manager", policy =>
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
                {
                    if (p.User?.Identity?.IsAuthenticated != true)
                    {
                        return false;
                    }

                    var claimsRole = p.User.Claims.Where(c => c.Type == "role");
                    var claimsScope = p.User.Claims.Where(c => c.Type == "scope");
                    if (!claimsRole.Any() && !claimsScope.Any())
                    {
                        return false;
                    }

                    return claimsRole.Any(c => c.Value == "administrator") || claimsScope.Any(c => c.Value == "manager");
                });
            });

            options.AddPolicy("Connected", policy => // User is connected
            {
                policy.AddAuthenticationSchemes(cookieName);
                policy.RequireAuthenticatedUser();
            });
            options.AddPolicy("registration", policy => // Access token with scope = register_client
            {
                policy.AddAuthenticationSchemes("OAuth2Introspection");
                policy.RequireClaim("scope", "register_client");
            });
            options.AddPolicy("connected_user", policy => // Introspect the identity token.
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection");
                policy.RequireAuthenticatedUser();
            });
            options.AddPolicy("manage_profile", policy => // Access token with scope = manage_profile or with role = administrator
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
            options.AddPolicy("manage_account_filtering", policy => // Access token with scope = manage_account_filtering or role = administrator
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
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

                    return claimRole != null && claimRole.Value == "administrator" ||
                           claimScopes.SelectMany(s => s.Value.Split(' ')).Any(s => s == "manage_account_filtering");
                });
            });
            return options;
        }

        public static IServiceCollection AddAccountFilter(this IServiceCollection services, List<Filter> filters = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IAccountFilter, AccountFilter>();
            services.AddSingleton<IFilterStore>(new DefaultFilterStore(filters));
            return services;
        }

        public static IServiceCollection UseSimpleAuth(
            this IServiceCollection services,
            SimpleAuthOptions options)
        {
            var s = services.AddSimpleAuth(options)
                 .AddSingleton(options.Scim)
                 .AddTransient<IRedirectInstructionParser, RedirectInstructionParser>()
                 .AddTransient<IActionResultParser, ActionResultParser>()
                 .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                 .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                 .AddDefaultTokenStore()
                 .AddTechnicalLogging()
                 .AddOpenidLogging()
                 .AddOAuthLogging();
            services.AddDataProtection();
            return s;
        }
    }
}