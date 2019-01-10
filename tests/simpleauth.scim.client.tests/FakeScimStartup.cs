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

namespace SimpleAuth.Scim.Client.Tests
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using MiddleWares;
    using Server.Extensions;
    using Services;
    using Shared;

    public class FakeScimStartup
    {
        public const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSimpleAuth();
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
            services.AddSingleton<ISubjectBuilder>(new DefaultSubjectBuilder());
            services.AddMvc();
            //var parts = mvc.PartManager.ApplicationParts;
            //parts.Clear();
            //parts.Add(new AssemblyPart(typeof(ResourceTypesController).GetTypeInfo().Assembly));
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
