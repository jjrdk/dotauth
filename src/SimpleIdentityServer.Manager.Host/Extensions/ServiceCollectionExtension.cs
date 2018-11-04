#region copyright
// Copyright 2015 Habart Thierry
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
#endregion

using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    using SimpleIdentityServer.Core.Common.AccountFiltering;
    using SimpleIdentityServer.Core.Common.Repositories;

    public static class ServiceCollectionExtension
    {
        public static void AddSimpleIdentityServerManager(this IServiceCollection serviceCollection, ManagerOptions managerOptions)
        {
            if (managerOptions == null)
            {
                throw new ArgumentNullException(nameof(managerOptions));
            }

            // 1. Add the dependencies needed to enable CORS
            serviceCollection.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 2. Register all the dependencies.
            serviceCollection.AddSimpleIdentityServerCore();
            serviceCollection.AddSimpleIdentityServerManagerCore();
            //serviceCollection.AddDefaultSimpleBus();
            //serviceCollection.AddConcurrency(opt => opt.UseInMemory());
            //serviceCollection.AddDefaultAccessTokenStore();
            // 3. Add authorization policies
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy("manager", policy =>
                {
					policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                    policy.RequireAssertion(p =>
                    {
                        if (p.User == null || p.User.Identity == null || !p.User.Identity.IsAuthenticated)
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
            });
            // 5. Add JWT parsers.
            serviceCollection.AddSimpleIdentityServerJwt();
            // 6. Add the dependencies needed to run MVC
			serviceCollection.AddTechnicalLogging();
			serviceCollection.AddManagerLogging();
			serviceCollection.AddOAuthLogging();
			serviceCollection.AddOpenidLogging();
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
    }
}
