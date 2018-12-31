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

namespace SimpleAuth.Server.Tests
{
    using System;
    using System.Reflection;
    using Controllers;
    using Extensions;
    using Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleAuth;

    public class FakeManagerStartup : IStartup
    {
        private const string DefaultSchema = "Cookies";
        
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            RegisterServices(services);
            var mvc = services.AddMvcCore(o => { }).AddJsonFormatters();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(JweController).GetTypeInfo().Assembly));
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
            serviceCollection.AddSingleton(new ScimOptions
            {
                IsEnabled = true,
                EndPoint = "http://localhost:5555/"
            });
            serviceCollection.AddSingleton<IOpenIdEventSource, OpenIdEventSource>();
            serviceCollection.AddSimpleAuthServer(resourceOwners: DefaultStorage.GetUsers());
            serviceCollection.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = DefaultSchema;
                opts.DefaultChallengeScheme = DefaultSchema;
            });
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy("manager", policy =>
                {
                    policy.RequireAssertion(p => true);
                });
            });
            //serviceCollection.AddDefaultAccessTokenStore();
            serviceCollection.AddSimpleAuthJwt();
            //serviceCollection.AddTechnicalLogging();
        }
    }
}
