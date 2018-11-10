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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using SimpleBus.Core;
using SimpleIdentityServer.AccessToken.Store;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core;
using SimpleIdentityServer.Manager.Host.Controllers;
using SimpleIdentityServer.Manager.Host.Extensions;
using SimpleIdentityServer.Manager.Logging;
using SimpleIdentityServer.OAuth.Logging;
using SimpleIdentityServer.OpenId.Logging;
using System;
using System.Reflection;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage;

namespace SimpleIdentityServer.Manager.Host.Tests
{
    public class FakeStartup : IStartup
    {
        public const string DefaultSchema = "Cookies";
        private SharedContext _context;

        public FakeStartup(SharedContext sharedContext)
        {
            _context = sharedContext;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            RegisterServices(services);
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(ConfigurationController).GetTypeInfo().Assembly));
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseManagerApi();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSimpleIdentityServerCore(resourceOwners: DefaultStorage.GetUsers());
            serviceCollection.AddSimpleIdentityServerManagerCore();
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy("manager", policy =>
                {
                    policy.RequireAssertion(p => true);
                });
            });
            serviceCollection.AddDefaultSimpleBus();
            serviceCollection.AddConcurrency(opt => opt.UseInMemory());
            serviceCollection.AddDefaultAccessTokenStore();
            serviceCollection.AddSimpleIdentityServerJwt();
            serviceCollection.AddTechnicalLogging();
            serviceCollection.AddManagerLogging();
            serviceCollection.AddOAuthLogging();
            serviceCollection.AddOpenidLogging();
        }
    }
}
