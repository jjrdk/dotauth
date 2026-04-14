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
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using DotAuth.Endpoints;
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
    /// Extension methods for <see cref="AuthorizationOptions"/>
    /// </summary>
    /// <param name="options">The <see cref="AuthorizationOptions"/> to invoke.</param>
    extension(AuthorizationOptions options)
    {
        /// <summary>
        /// Adds the authentication policies.
        /// </summary>
        /// <param name="administratorRoleDefinition"></param>
        /// <param name="authenticationSchemes">The authentication schemes.</param>
        /// <returns>The invoked <see cref="AuthorizationOptions"/>.</returns>
        /// <exception cref="ArgumentNullException">options</exception>
        public AuthorizationOptions AddAuthPolicies(
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
                    policy.RequireAssertion(p =>
                    {
                        if (p.User.Identity?.IsAuthenticated != true)
                        {
                            return false;
                        }

                        if (HasLocalUiSession(p.User))
                        {
                            return true;
                        }

                        if (p.User.Claims.Where(c => c.Type == ScopeType)
                            .Any(c => c.HasClaimValue("uma_protection")))
                        {
                            return true;
                        }

                        var claimScopes = p.User.Claims.FirstOrDefault(c => c.Type == ScopeType);
                        return claimScopes != null
                         && claimScopes.Value.Split(' ', StringSplitOptions.TrimEntries)
                                .Any(s => s == "uma_protection");
                    });
                });
            options.AddPolicy(
                "dcr",
                policy =>
                {
                    policy.AddAuthenticationSchemes(authenticationSchemes);
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(p =>
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
                    policy.RequireAssertion(p =>
                    {
                        if (p.User.Identity?.IsAuthenticated != true)
                        {
                            return false;
                        }

                        if (HasLocalUiSession(p.User) && HasAdministratorRole(p.User, administratorRoleDefinition))
                        {
                            return true;
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

        private static bool HasLocalUiSession(ClaimsPrincipal principal)
        {
            return principal.Identities.Any(identity =>
                identity is { IsAuthenticated: true }
                && string.Equals(identity.AuthenticationType, CookieNames.CookieName, StringComparison.Ordinal));
        }

        private static bool HasAdministratorRole(
            ClaimsPrincipal principal,
            (string roleName, string roleClaim) administratorRoleDefinition)
        {
            if (administratorRoleDefinition == default)
            {
                return false;
            }

            var (roleName, roleClaim) = administratorRoleDefinition;
            return principal.Claims.Where(c => c.Type == roleName).Any(c => c.HasClaimValue(roleClaim));
        }
    }

    /// <param name="services">The services.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the account filter.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">services</exception>
        public IServiceCollection AddAccountFilter(params Filter[] filters)
        {
            services.AddTransient<IAccountFilter, AccountFilter>();
            services.AddSingleton<IFilterStore>(new InMemoryFilterStore(filters));
            return services;
        }

        /// <summary>
        /// Adds DotAuth type registrations.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mvcConfig">MVC configuration.</param>
        /// <param name="requestThrottle">The rate limiter.</param>
        /// <param name="authenticationSchemes"></param>
        /// <returns>An <see cref="IMvcCoreBuilder"/> instance.</returns>
        public IMvcCoreBuilder AddDotAuthServer(
            Action<DotAuthConfiguration> configuration,
            string[] authenticationSchemes,
            Action<MvcOptions>? mvcConfig = null,
            IRequestThrottle? requestThrottle = null)
        {
            var options = new DotAuthConfiguration();
            configuration(options);

            return AddDotAuthServer(services, options, authenticationSchemes, mvcConfig, requestThrottle);
        }

        /// <summary>
        /// Adds DotAuth type registrations.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="mvcConfig">MVC configuration.</param>
        /// <param name="requestThrottle">The rate limiter.</param>
        /// <param name="authenticationSchemes"></param>
        /// <returns>An <see cref="IMvcCoreBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">options</exception>
        public IMvcCoreBuilder AddDotAuthServer(
            DotAuthConfiguration configuration,
            string[] authenticationSchemes,
            Action<MvcOptions>? mvcConfig = null,
            IRequestThrottle? requestThrottle = null)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddResponseCompression(o =>
                {
                    o.EnableForHttps = true;
                    o.Providers.Add(
                        new GzipCompressionProvider(
                            new GzipCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                    o.Providers.Add(
                        new BrotliCompressionProvider(
                            new BrotliCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                })
                .ConfigureHttpJsonOptions(o =>
                {
                    ApplySharedJsonOptions(o.SerializerOptions);
                })
                .AddAntiforgery(o =>
                {
                    o.FormFieldName = "XrsfField";
                    o.HeaderName = "XSRF-TOKEN";
                    o.SuppressXFrameOptionsHeader = false;
                })
                .AddCors(o => o.AddPolicy("CorsDefault",
                    p => p.WithOrigins(configuration.AllowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()));

            var mvcBuilder = services.AddMvcCore(o => { mvcConfig?.Invoke(o); })
                .AddViews()
                .AddRazorViewEngine();

            Globals.ApplicationName = configuration.ApplicationName;
            var runtimeConfig = GetRuntimeConfig(configuration);
            services.AddAuthentication();
            services.AddAuthorization(opts =>
            {
                opts.AddAuthPolicies(configuration.AdministratorRoleDefinition, authenticationSchemes);
            });

            var s = services.AddTransient<IAuthenticateResourceOwnerService, UsernamePasswordAuthenticationService>()
                .AddTransient<ITwoFactorAuthenticationHandler, TwoFactorAuthenticationHandler>()
                .AddSingleton<IAuthorizationPolicyValidator, AuthorizationPolicyValidator>()
                .AddSingleton(runtimeConfig)
                .AddSingleton(requestThrottle ?? NoopThrottle.Instance)
                .AddSingleton(sp => configuration.EventPublisher?.Invoke(sp) ?? new NoopEventPublisher())
                .AddSingleton(sp => configuration.SubjectBuilder?.Invoke(sp) ?? new DefaultSubjectBuilder())
                .AddSingleton(sp => configuration.JsonWebKeys?.Invoke(sp) ?? new InMemoryJwksRepository())
                .AddSingleton<IJwksStore>(sp => sp.GetRequiredService<IJwksRepository>())
                .AddSingleton(sp => configuration.Clients?.Invoke(sp)
                 ?? new InMemoryClientRepository(
                        sp.GetRequiredService<IHttpClientFactory>(),
                        sp.GetRequiredService<IScopeStore>(),
                        sp.GetRequiredService<ILogger<InMemoryClientRepository>>()))
                .AddSingleton<IClientStore>(sp => sp.GetRequiredService<IClientRepository>())
                .AddSingleton(sp => configuration.Consents?.Invoke(sp) ?? new InMemoryConsentRepository())
                .AddSingleton<IConsentStore>(sp => sp.GetRequiredService<IConsentRepository>())
                .AddSingleton(sp =>
                    configuration.Users?.Invoke(sp) ?? new InMemoryResourceOwnerRepository(configuration.Salt))
                .AddSingleton<IResourceOwnerStore>(sp => sp.GetRequiredService<IResourceOwnerRepository>())
                .AddSingleton(sp => configuration.Scopes?.Invoke(sp) ?? new InMemoryScopeRepository())
                .AddSingleton<IScopeStore>(sp => sp.GetRequiredService<IScopeRepository>())
                .AddSingleton(sp =>
                    configuration.DeviceAuthorizations?.Invoke(sp) ?? new InMemoryDeviceAuthorizationStore())
                .AddSingleton(sp =>
                    configuration.ResourceSets?.Invoke(sp) ??
                    new InMemoryResourceSetRepository(sp.GetRequiredService<IAuthorizationPolicy>()))
                .AddSingleton(sp => configuration.Tickets?.Invoke(sp) ?? new InMemoryTicketStore())
                .AddSingleton(sp =>
                    configuration.AuthorizationCodes?.Invoke(sp) ?? new InMemoryAuthorizationCodeStore())
                .AddSingleton(sp => configuration.Tokens?.Invoke(sp) ?? new InMemoryTokenStore())
                .AddSingleton(sp => configuration.ConfirmationCodes?.Invoke(sp) ?? new InMemoryConfirmationCodeStore())
                .AddSingleton(sp => configuration.AccountFilters?.Invoke(sp) ?? new InMemoryFilterStore())
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IAuthorizationPolicy, DefaultAuthorizationPolicy>();
            if (configuration.DataProtector != null)
            {
                // Register the provided IDataProtector and expose it through an
                // IDataProtectionProvider wrapper so controllers that depend on
                // IDataProtectionProvider work as expected.
                s.AddSingleton<IDataProtector>(sp => configuration.DataProtector(sp));
                s.AddSingleton<IDataProtectionProvider>(
                    sp => new DataProtectorProviderWrapper(sp.GetRequiredService<IDataProtector>()));
            }
            else
            {
                s.AddDataProtection();
            }

            // Simple adapter that exposes an IDataProtector as an
            // IDataProtectionProvider. CreateProtector returns the underlying
            // protector regardless of purpose because the configured
            // IDataProtector is expected to already handle protection logic.

            return mvcBuilder;
        }
    }

    /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Registers the mvc routes for a DotAuth server.
        /// </summary>
        /// <param name="forwardedHeaderConfiguration">Configuration action for handling proxy setup.</param>
        /// <param name="applicationTypes">Additional types to discover view assemblies.</param>
        /// <returns></returns>
        public IApplicationBuilder UseDotAuthServer(
            Action<ForwardedHeadersOptions>? forwardedHeaderConfiguration = null,
            params IEnumerable<Type> applicationTypes)
        {
            return app.UseDotAuthServer(
                forwardedHeaderConfiguration,
                applicationTypes.Select(type => (type.Namespace ?? string.Empty, type.Assembly)).ToArray());
        }

        /// <summary>
        /// Registers the mvc routes for a DotAuth server.
        /// </summary>
        /// <param name="forwardedHeaderConfiguration">Configuration action for handling proxy setup.</param>
        /// <param name="assemblies">Additional view assemblies.</param>
        /// <returns></returns>
        public IApplicationBuilder UseDotAuthServer(
            Action<ForwardedHeadersOptions>? forwardedHeaderConfiguration = null,
            params (string defaultNamespace, Assembly assembly)[] assemblies)
        {
            var publisher = app.ApplicationServices.GetService(typeof(IEventPublisher)) ?? new NoOpPublisher();
            var forwardedHeadersOptions = new ForwardedHeadersOptions
                { ForwardedHeaders = ForwardedHeaders.All, ForwardLimit = 1 };
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
                                $"public,max-age={TimeSpan.FromDays(7).TotalSeconds}";
                        },
                        FileProvider = new CompositeFileProvider(
                            assemblies.Select(x => new EmbeddedFileProvider(x.assembly, x.defaultNamespace)))
                    })
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseCors("CorsDefault")
                .UseEndpoints(endpoint =>
                {
                    endpoint.MapDotAuthUiEndpoints();
                    endpoint.MapDotAuthApiEndpoints();
                });
        }
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

    // Simple adapter that exposes an IDataProtector as an
    // IDataProtectionProvider. CreateProtector returns the underlying
    // protector regardless of purpose because the configured
    // IDataProtector is expected to already handle protection logic.
    private sealed class DataProtectorProviderWrapper : IDataProtectionProvider
    {
        private readonly IDataProtector _protector;

        public DataProtectorProviderWrapper(IDataProtector protector) => _protector = protector;

        public IDataProtector CreateProtector(string purpose) => _protector;
    }

    private static void ApplySharedJsonOptions(System.Text.Json.JsonSerializerOptions options)
    {
        var instance = SharedSerializerContext.Default.Options;
        foreach (var converter in instance.Converters)
        {
            options.Converters.Add(converter);
        }

        // NOTE: copying the generated TypeInfoResolver and its chain can lead to
        // recursive resolver references and stack overflows during application
        // startup (observed in test host). To be safe, only copy converter
        // instances and simple serializer settings here. Avoid copying the
        // TypeInfoResolverChain/TypeInfoResolver which may reference back into
        // serializer options and create recursion.
        options.NumberHandling = instance.NumberHandling;
        options.WriteIndented = instance.WriteIndented;
        options.AllowTrailingCommas = instance.AllowTrailingCommas;
        options.DefaultIgnoreCondition = instance.DefaultIgnoreCondition;
        options.DictionaryKeyPolicy = instance.DictionaryKeyPolicy;
        options.PropertyNamingPolicy = instance.PropertyNamingPolicy;
        options.ReadCommentHandling = instance.ReadCommentHandling;
        // Do NOT set options.TypeInfoResolver = instance.TypeInfoResolver;
        // Setting the TypeInfoResolver from the generated context has caused
        // stack-overflow crashes in the test host on some platforms. Keep the
        // existing resolver on 'options' so the runtime uses the configured
        // behavior without introducing resolver cycles.
        options.UnknownTypeHandling = instance.UnknownTypeHandling;
        options.IgnoreReadOnlyFields = instance.IgnoreReadOnlyFields;
        options.IgnoreReadOnlyProperties = instance.IgnoreReadOnlyProperties;
        options.PropertyNameCaseInsensitive = instance.PropertyNameCaseInsensitive;
    }
}
