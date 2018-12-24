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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Uma.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Uma.Host.Extensions
{
    using SimpleAuth.Shared.Models;

    public static class ServiceCollectionExtensions
    {
        private static readonly List<Scope> DEFAULT_SCOPES = new List<Scope>
        {
            new Scope
            {
                Name = "uma_protection",
                Description = "Access to UMA permission, resource set",
                IsOpenIdScope = false,
                IsDisplayedInConsent = false,
                Type = ScopeType.ProtectedApi,
                UpdateDateTime = DateTime.MinValue,
                CreateDateTime = DateTime.MinValue
            }
        };

        public static IServiceCollection AddUmaHost(this IServiceCollection services, AuthorizationServerOptions authorizationServerOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // 1. Add the dependencies.
            RegisterServices(services, authorizationServerOptions);
            return services;
        }

        public static AuthorizationOptions AddUmaSecurityPolicy(this AuthorizationOptions authorizationOptions)
        {
            if (authorizationOptions == null)
            {
                throw new ArgumentNullException(nameof(authorizationOptions));
            }

            authorizationOptions.AddPolicy("UmaProtection", policy =>
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
                {
                    if (p.User?.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRole = p.User.Claims.Where(c => c.Type == ClaimTypes.Role);
                    var claimScopes = p.User.Claims.Where(c => c.Type == "scope");
                    if (claimRole == null && !claimScopes.Any())
                    {
                        return false;
                    }

                    return claimRole != null && claimRole.Any(role => role.Value == "administrator") || claimScopes.Any(s => s.Value == "uma_protection");
                });
            });
            return authorizationOptions;
        }

        private static void RegisterServices(IServiceCollection services, AuthorizationServerOptions authorizationServerOptions)
        {
            services.AddSimpleIdServerUmaCore(
                authorizationServerOptions.UmaConfigurationOptions,
                    authorizationServerOptions.Configuration?.Resources,
                    authorizationServerOptions.Configuration?.Policies)
                .AddSimpleIdentityServerCore(
                authorizationServerOptions.OAuthConfigurationOptions,
                    clients: authorizationServerOptions.Configuration?.Clients,
                    scopes: authorizationServerOptions.Configuration == null
                        ? DEFAULT_SCOPES
                        : authorizationServerOptions.Configuration.Scopes,
                    claims: new List<ClaimAggregate>())
                .AddSimpleIdentityServerJwt()
                //.AddIdServerClient()
                .AddDefaultTokenStore();
            //.AddDefaultSimpleBus()
            //.AddConcurrency(opt => opt.UseInMemory());
            services.AddTechnicalLogging();
            services.AddOAuthLogging();
            services.AddUmaLogging();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUmaServerEventSource, UmaServerEventSource>();
        }
    }
}