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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Client.Tests.MiddleWares;
using SimpleIdentityServer.Scim.Host.Controllers;
using SimpleIdentityServer.Scim.Host.Extensions;
using System.Reflection;

namespace SimpleIdentityServer.Scim.Client.Tests
{
    using SimpleAuth.Shared;
    using SimpleIdentityServer.Core;
    using SimpleIdentityServer.Core.Logging;
    using SimpleIdentityServer.Core.Services;
    using SimpleIdentityServer.Core.WebSite.User.Actions;

    public class FakeStartup
    {
        public const string DefaultSchema = "Cookies";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSimpleIdentityServerCore()
                .AddOpenidLogging()
                .AddScimHost(new Host.ScimServerConfiguration
            {

            });
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = DefaultSchema;
                opts.DefaultChallengeScheme = DefaultSchema;
            }).AddFakeCustomAuth(o => { });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(ScimConstants.ScimPolicies.ScimManage, policy => policy.RequireAssertion((ctx) => true));
                options.AddPolicy(ScimConstants.ScimPolicies.ScimRead, policy => policy.RequireAssertion((ctx) => true));
                options.AddPolicy("authenticated", (policy) =>
                {
                    policy.AddAuthenticationSchemes(DefaultSchema);
                    policy.RequireAuthenticatedUser();
                });
            });
            services.AddTransient<IAddUserOperation, AddUserOperation>();
            //services.AddSingleton(sp => new Mock<IResourceOwnerRepository>().Object);
            //services.AddSingleton(sp => new Mock<IClaimRepository>().Object);
            services.AddTransient<IEventPublisher, DefaultEventPublisher>();
            services.AddSingleton<ISubjectBuilder>(new DefaultSubjectBuilder());
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(ResourceTypesController).GetTypeInfo().Assembly));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
