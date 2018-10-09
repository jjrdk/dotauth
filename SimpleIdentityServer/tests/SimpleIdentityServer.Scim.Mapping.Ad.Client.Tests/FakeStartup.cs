using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Mapping.Ad.Controllers;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Tests
{
    public class FakeStartup
    {
        private List<AdMapping> DEFAULT_MAPPINGS = new List<AdMapping>
        {
            new AdMapping
            {
                AdPropertyName = "property",
                AttributeId = "attributeid",
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            }
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("scim_manage", policy => policy.RequireAssertion((ctx) => {
                    return true;
                }));
            });
            services.AddScimMapping(adMappings: DEFAULT_MAPPINGS);
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(AdConfigurationController).GetTypeInfo().Assembly));
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
