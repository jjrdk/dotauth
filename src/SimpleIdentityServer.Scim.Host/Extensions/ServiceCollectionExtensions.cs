using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace SimpleIdentityServer.Scim.Host.Extensions
{
    using Core.Validators;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimHost(this IServiceCollection services, ScimServerConfiguration scimServerOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IParametersValidator, ParametersValidator>();
            services.AddSingleton(new InMemoryGroupsRepository());
            services.AddSingleton<IProvide<GroupResource>>(sp => sp.GetService<InMemoryGroupsRepository>());
            services.AddSingleton<IPersist<GroupResource>>(sp => sp.GetService<InMemoryGroupsRepository>());
            services.AddSingleton<IStore<GroupResource>>(sp => sp.GetService<InMemoryGroupsRepository>());
            return services;
        }

        public static AuthorizationOptions AddScimAuthPolicy(this AuthorizationOptions options)
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
                policy.AddAuthenticationSchemes("UserInfoIntrospection");
                policy.RequireAuthenticatedUser();
            });
            return options;
        }
    }
}
