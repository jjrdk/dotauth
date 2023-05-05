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

namespace DotAuth;

using System;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using DotAuth.Controllers;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Filters;
using DotAuth.MiddleWare;
using DotAuth.Policies;
using DotAuth.Repositories;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Policies;
using DotAuth.Shared.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

/// <summary>
/// Defines the service collection extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string ScopeType = "scope";

    /// <summary>
    /// Adds the authentication policies.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="administratorRoleDefinition"></param>
    /// <param name="authenticationSchemes">The authentication schemes.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">options</exception>
    public static AuthorizationOptions AddAuthPolicies(
        this AuthorizationOptions options,
        (string roleName, string roleClaim) administratorRoleDefinition,
        params string[] authenticationSchemes)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.AddPolicy(
            "authenticated",
            policy =>
            {
                policy.AddAuthenticationSchemes(authenticationSchemes);
                policy.RequireAuthenticatedUser();
            });
        options.AddPolicy(
            "UmaProtection",
            policy =>
            {
                policy.AddAuthenticationSchemes(authenticationSchemes);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(
                    p =>
                    {
                        if (p.User.Identity?.IsAuthenticated != true)
                        {
                            return false;
                        }

                        if (p.User.Claims.Where(c => c.Type == ScopeType)
                            .Any(c => c.HasClaimValue("uma_protection")))
                        {
                            return true;
                        }

                        var claimScopes = p.User.Claims.FirstOrDefault(c => c.Type == ScopeType);
                        return claimScopes != null
                               && claimScopes.Value.Split(' ', StringSplitOptions.TrimEntries).Any(s => s == "uma_protection");
                    });
            });
        options.AddPolicy(
            "dcr",
            policy =>
            {
                policy.AddAuthenticationSchemes(authenticationSchemes);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(
                    p =>
                    {
                        if (p.User.Identity?.IsAuthenticated != true)
                        {
                            return false;
                        }

                        if (p.User.Claims.Where(c => c.Type == ScopeType)
                            .Any(c => c.HasClaimValue("dcr")))
                        {
                            return true;
                        }

                        var claimScopes = p.User.Claims.FirstOrDefault(c => c.Type == ScopeType);
                        return claimScopes != null
                               && claimScopes.Value.Split(' ', StringSplitOptions.TrimEntries).Any(s => s == "dcr");
                    });
            });
        options.AddPolicy(
            "manager",
            policy =>
            {
                policy.AddAuthenticationSchemes(authenticationSchemes);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(
                    p =>
                    {
                        if (p.User.Identity?.IsAuthenticated != true)
                        {
                            return false;
                        }

                        var result =
                            p.User.Claims.Where(c => c.Type == ScopeType).Any(c => c.HasClaimValue("manager"));
                        if (administratorRoleDefinition == default)
                        {
                            return result;
                        }

                        var (roleName, roleClaim) = administratorRoleDefinition;
                        return result
                               && p.User.Claims.Where(c => c.Type == roleName)
                                   .Any(c => c.HasClaimValue(roleClaim));
                    });
            });

        return options;
    }

    /// <summary>
    /// Adds the account filter.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="filters">The filters.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">services</exception>
    public static IServiceCollection AddAccountFilter(this IServiceCollection services, params Filter[] filters)
    {
        services.AddTransient<IAccountFilter, AccountFilter>();
        services.AddSingleton<IFilterStore>(new InMemoryFilterStore(filters));
        return services;
    }

    /// <summary>
    /// Adds DotAuth type registrations.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="mvcConfig">MVC configuration.</param>
    /// <param name="requestThrottle">The rate limiter.</param>
    /// <param name="authPolicies"></param>
    /// <returns>An <see cref="IMvcBuilder"/> instance.</returns>
    public static IMvcBuilder AddDotAuthServer(
        this IServiceCollection services,
        Action<DotAuthConfiguration> configuration,
        string[] authPolicies,
        Action<MvcOptions>? mvcConfig = null,
        IRequestThrottle? requestThrottle = null)
    {
        var options = new DotAuthConfiguration();
        configuration(options);

        return AddDotAuthServer(services, options, authPolicies, mvcConfig, requestThrottle, Array.Empty<Type>());
    }

    /// <summary>
    /// Adds DotAuth type registrations.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="options">The options.</param>
    /// <param name="mvcConfig">MVC configuration.</param>
    /// <param name="requestThrottle">The rate limiter.</param>
    /// <param name="authPolicies"></param>
    /// <param name="assemblyTypes">Assemblies with additional application parts.</param>
    /// <returns>An <see cref="IMvcBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">options</exception>
    public static IMvcBuilder AddDotAuthServer(
        this IServiceCollection services,
        DotAuthConfiguration configuration,
        string[] authPolicies,
        Action<MvcOptions>? mvcConfig = null,
        IRequestThrottle? requestThrottle = null,
        params Type[] assemblyTypes)
    {
        return AddDotAuthServer(
            services,
            configuration,
            authPolicies,
            mvcConfig,
            requestThrottle,
            assemblyTypes.Select(type => (type.Namespace ?? string.Empty, type.Assembly)).ToArray());
    }

    /// <summary>
    /// Adds DotAuth type registrations.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="options">The options.</param>
    /// <param name="mvcConfig">MVC configuration.</param>
    /// <param name="requestThrottle">The rate limiter.</param>
    /// <param name="authPolicies"></param>
    /// <param name="assemblies">Assemblies with additional application parts.</param>
    /// <returns>An <see cref="IMvcBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">options</exception>
    public static IMvcBuilder AddDotAuthServer(
        this IServiceCollection services,
        DotAuthConfiguration configuration,
        string[] authPolicies,
        Action<MvcOptions>? mvcConfig = null,
        IRequestThrottle? requestThrottle = null,
        params (string defaultNamespace, Assembly assembly)[] assemblies)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        services.Replace(
            new ServiceDescriptor(
                typeof(IActionResultExecutor<ObjectResult>),
                typeof(ConnegObjectResultExecutor),
                ServiceLifetime.Transient));
        var mvcBuilder = services.AddResponseCompression(
                o =>
                {
                    o.EnableForHttps = true;
                    o.Providers.Add(
                        new GzipCompressionProvider(
                            new GzipCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                    o.Providers.Add(
                        new BrotliCompressionProvider(
                            new BrotliCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                })
            .AddAntiforgery(
                o =>
                {
                    o.FormFieldName = "XrsfField";
                    o.HeaderName = "XSRF-TOKEN";
                    o.SuppressXFrameOptionsHeader = false;
                })
            .AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()))
            .AddControllersWithViews(
                o =>
                {
                    o.OutputFormatters.Add(new RazorOutputFormatter());
                    mvcConfig?.Invoke(o);
                });
        mvcBuilder = assemblies.Distinct()
            .Aggregate(
                mvcBuilder,
                (b, a) =>
                {
                    return b.AddRazorRuntimeCompilation(
                            o => o.FileProviders.Add(new EmbeddedFileProvider(a.assembly, a.defaultNamespace)))
                        .AddApplicationPart(a.assembly);
                })
            .AddNewtonsoftJson();
        Globals.ApplicationName = configuration.ApplicationName;
        var runtimeConfig = GetRuntimeConfig(configuration);
        services.AddAuthentication();
        services.AddAuthorization(opts => { opts.AddAuthPolicies(configuration.AdministratorRoleDefinition, authPolicies); });

        var s = services.AddTransient<IAuthenticateResourceOwnerService, UsernamePasswordAuthenticationService>()
            .AddTransient<ITwoFactorAuthenticationHandler, TwoFactorAuthenticationHandler>()
            .ConfigureOptions<ConfigureMvcNewtonsoftJsonOptions>()
            .AddSingleton<IAuthorizationPolicyValidator, AuthorizationPolicyValidator>()
            .AddSingleton(runtimeConfig)
            .AddSingleton(requestThrottle ?? NoopThrottle.Instance)
            .AddSingleton(sp => configuration.EventPublisher?.Invoke(sp) ?? new NoopEventPublisher())
            .AddSingleton(sp => configuration.SubjectBuilder?.Invoke(sp) ?? new DefaultSubjectBuilder())
            .AddSingleton(sp => configuration.JsonWebKeys?.Invoke(sp) ?? new InMemoryJwksRepository())
            .AddSingleton<IJwksStore>(sp => sp.GetRequiredService<IJwksRepository>())
            .AddSingleton(
                sp => configuration.Clients?.Invoke(sp)
                      ?? new InMemoryClientRepository(
                          sp.GetRequiredService<IHttpClientFactory>(),
                          sp.GetRequiredService<IScopeStore>(),
                          sp.GetRequiredService<ILogger<InMemoryClientRepository>>()))
            .AddSingleton<IClientStore>(sp => sp.GetRequiredService<IClientRepository>())
            .AddSingleton(sp => configuration.Consents?.Invoke(sp) ?? new InMemoryConsentRepository())
            .AddSingleton<IConsentStore>(sp => sp.GetRequiredService<IConsentRepository>())
            .AddSingleton(sp => configuration.Users?.Invoke(sp) ?? new InMemoryResourceOwnerRepository(configuration.Salt))
            .AddSingleton<IResourceOwnerStore>(sp => sp.GetRequiredService<IResourceOwnerRepository>())
            .AddSingleton(sp => configuration.Scopes?.Invoke(sp) ?? new InMemoryScopeRepository())
            .AddSingleton<IScopeStore>(sp => sp.GetRequiredService<IScopeRepository>())
            .AddSingleton(sp => configuration.DeviceAuthorizations?.Invoke(sp) ?? new InMemoryDeviceAuthorizationStore())
            .AddSingleton(sp => configuration.ResourceSets?.Invoke(sp) ?? new InMemoryResourceSetRepository(sp.GetRequiredService<IAuthorizationPolicy>()))
            .AddSingleton(sp => configuration.Tickets?.Invoke(sp) ?? new InMemoryTicketStore())
            .AddSingleton(sp => configuration.AuthorizationCodes?.Invoke(sp) ?? new InMemoryAuthorizationCodeStore())
            .AddSingleton(sp => configuration.Tokens?.Invoke(sp) ?? new InMemoryTokenStore())
            .AddSingleton(sp => configuration.ConfirmationCodes?.Invoke(sp) ?? new InMemoryConfirmationCodeStore())
            .AddSingleton(sp => configuration.AccountFilters?.Invoke(sp) ?? new InMemoryFilterStore())
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
            .AddSingleton<IAuthorizationPolicy, DefaultAuthorizationPolicy>();
        if (configuration.DataProtector != null)
        {
            s.AddSingleton(configuration.DataProtector);
            s.AddTransient<IDataProtectionProvider>(sp => sp.GetRequiredService<IDataProtector>());
        }
        else
        {
            s.AddDataProtection();
        }

        return mvcBuilder;
    }

    /// <summary>
    /// Registers the mvc routes for a DotAuth server.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
    /// <param name="forwardedHeaderConfiguration">Configuration action for handling proxy setup.</param>
    /// <param name="applicationTypes">Additional types to discover view assemblies.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseDotAuthServer(
        this IApplicationBuilder app,
        Action<ForwardedHeadersOptions>? forwardedHeaderConfiguration = null,
        params Type[] applicationTypes)
    {
        return app.UseDotAuthServer(
            forwardedHeaderConfiguration,
            applicationTypes.Select(type => (type.Namespace ?? string.Empty, type.Assembly)).ToArray());
    }

    /// <summary>
    /// Registers the mvc routes for a DotAuth server.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
    /// <param name="forwardedHeaderConfiguration">Configuration action for handling proxy setup.</param>
    /// <param name="assemblies">Additional view assemblies.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseDotAuthServer(
        this IApplicationBuilder app,
        Action<ForwardedHeadersOptions>? forwardedHeaderConfiguration = null,
        params (string defaultNamespace, Assembly assembly)[] assemblies)
    {
        var publisher = app.ApplicationServices.GetService(typeof(IEventPublisher)) ?? new NoOpPublisher();
        var forwardedHeadersOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All, ForwardLimit = null };
        forwardedHeadersOptions.KnownNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();
        forwardedHeaderConfiguration?.Invoke(forwardedHeadersOptions);
        return app
            .UseForwardedHeaders(forwardedHeadersOptions)
            .UseMiddleware<ExceptionHandlerMiddleware>(publisher)
            .UseResponseCompression()
            .UseStaticFiles(
                new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                            "public,max-age=" + TimeSpan.FromDays(7).TotalSeconds;
                    },
                    FileProvider = new CompositeFileProvider(
                        assemblies.Select(x => new EmbeddedFileProvider(x.assembly, x.defaultNamespace)))
                })
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseCors("AllowAll")
            .UseEndpoints(
                endpoint =>
                {
                    //endpoint.MapRazorPages();
                    endpoint.MapControllerRoute("areaexists", "{area:exists}/{controller=Home}/{action=Index}");
                    endpoint.MapControllerRoute("pwdauth", "pwd/{controller=Home}/{action=Index}");
                    endpoint.MapControllerRoute("default", "{controller=Home}/{action=Index}");
                });
    }

    private static RuntimeSettings GetRuntimeConfig(DotAuthConfiguration configuration)
    {
        return new(
            configuration.Salt,
            onResourceOwnerCreated: configuration.OnResourceOwnerCreated,
            authorizationCodeValidityPeriod: configuration.AuthorizationCodeValidityPeriod,
            claimsIncludedInUserCreation: configuration.ClaimsIncludedInUserCreation,
            rptLifeTime: configuration.RptLifeTime,
            patLifeTime: configuration.PatLifeTime,
            ticketLifeTime: configuration.TicketLifeTime,
            devicePollingInterval: configuration.DevicePollingInterval,
            deviceAuthorizationLifetime: configuration.DeviceAuthorizationLifetime,
            allowHttp: configuration.AllowHttp,
            redirectToLogin: configuration.RedirectToLogin);
    }
}