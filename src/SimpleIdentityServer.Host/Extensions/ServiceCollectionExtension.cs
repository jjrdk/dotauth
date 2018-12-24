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
    using Microsoft.Extensions.DependencyInjection;
    using Shared;
    using Shared.AccountFiltering;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSimpleIdentityServerManager(this IServiceCollection serviceCollection)
        {
            // 2. Register all the dependencies.
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
            });

            return serviceCollection;
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
