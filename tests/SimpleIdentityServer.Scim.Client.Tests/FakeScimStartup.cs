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

namespace SimpleIdentityServer.Scim.Client.Tests
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleIdentityServer.Scim.Client.Tests.MiddleWares;
    using System.Reflection;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using SimpleAuth;
    using SimpleAuth.Logging;
    using SimpleAuth.Server.Controllers;
    using SimpleAuth.Server.Extensions;
    using SimpleAuth.Services;
    using SimpleAuth.Shared;
    using SimpleAuth.WebSite.User.Actions;

    public class FakeScimStartup
    {
        public const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSimpleAuthServer()
                .AddOpenidLogging()
                .AddScimHost();
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = DefaultSchema;
                opts.DefaultChallengeScheme = DefaultSchema;
            }).AddFakeCustomAuth(o => { });
            services.AddAuthorization(options =>
            {
                options.AddAuthPolicies(DefaultSchema);
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
