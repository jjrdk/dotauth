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

namespace SimpleAuth.Extensions
{
    using Api.Token;
    using Common;
    using Helpers;
    using JwtToken;
    using Logging;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Parsers;
    using Policies;
    using Repositories;
    using Services;
    using Shared;
    using Shared.AccountFiltering;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using Translation;
    using Validators;
    using WebSite.Authenticate;
    using WebSite.Authenticate.Actions;
    using WebSite.Authenticate.Common;
    using WebSite.Consent;
    using WebSite.Consent.Actions;

    public static class ServiceCollectionExtensions
    {
        //private static readonly List<Scope> DEFAULT_SCOPES = new List<Scope>
        //{
        //    new Scope
        //    {
        //        Name = "uma_protection",
        //        Description = "Access to UMA permission, resource set",
        //        IsOpenIdScope = false,
        //        IsDisplayedInConsent = false,
        //        Type = ScopeType.ProtectedApi,
        //        UpdateDateTime = DateTime.MinValue,
        //        CreateDateTime = DateTime.MinValue
        //    }
        //};

        public static IServiceCollection AddDefaultTokenStore(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IAuthorizationCodeStore>(new InMemoryAuthorizationCodeStore());
            serviceCollection.AddSingleton<ITokenStore>(new InMemoryTokenStore());
            serviceCollection.AddSingleton<IConfirmationCodeStore>(new InMemoryConfirmationCode());
            return serviceCollection;
        }

        public static AuthorizationOptions AddAuthPolicies(this AuthorizationOptions options, string cookieName)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }


            options.AddPolicy("UmaProtection", policy =>
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
                {
                    if (p.User?.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRole = p.User.Claims.Where(c => c.Type == ClaimTypes.Role);
                    var claimScopes = p.User.Claims.Where(c => c.Type == "scope");
                    if (claimRole == null && !claimScopes.Any())
                    {
                        return false;
                    }

                    return claimRole.Any(role => role.Value == "administrator") || claimScopes.Any(s => s.Value == "uma_protection");
                });
            });
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
                //policy.AddAuthenticationSchemes("UserInfoIntrospection");
                policy.RequireAuthenticatedUser();
            });
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

            options.AddPolicy("Connected", policy => // User is connected
            {
                policy.AddAuthenticationSchemes(cookieName);
                policy.RequireAuthenticatedUser();
            });
            options.AddPolicy("registration", policy => // Access token with scope = register_client
            {
                policy.AddAuthenticationSchemes("OAuth2Introspection");
                policy.RequireClaim("scope", "register_client");
            });
            options.AddPolicy("connected_user", policy => // Introspect the identity token.
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection");
                policy.RequireAuthenticatedUser();
            });
            options.AddPolicy("manage_profile", policy => // Access token with scope = manage_profile or with role = administrator
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

                    return claimRole != null && claimRole.Value == "administrator" || claimScopes.Any(s => s.Value == "manage_profile");
                });
            });
            options.AddPolicy("manage_account_filtering", policy => // Access token with scope = manage_account_filtering or role = administrator
            {
                policy.AddAuthenticationSchemes("UserInfoIntrospection", "OAuth2Introspection");
                policy.RequireAssertion(p =>
                {
                    if (p.User?.Identity == null || !p.User.Identity.IsAuthenticated)
                    {
                        return false;
                    }

                    var claimRole = p.User.Claims.FirstOrDefault(c => c.Type == "role");
                    var claimScopes = p.User.Claims.Where(c => c.Type == "scope").ToArray();
                    if (claimRole == null && !claimScopes.Any())
                    {
                        return false;
                    }

                    return claimRole != null && claimRole.Value == "administrator" ||
                           claimScopes.SelectMany(s => s.Value.Split(' ')).Any(s => s == "manage_account_filtering");
                });
            });
            return options;
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

        public static IServiceCollection AddSimpleAuth(
            this IServiceCollection services,
            SimpleAuthOptions options = null)
        {
            var s = services
            .AddTransient<IAuthenticateResourceOwnerService, UsernamePasswordAuthenticationService>()
            .AddTransient<IUpdateResourceOwnerClaimsParameterValidator, UpdateResourceOwnerClaimsParameterValidator>()
            .AddTransient<IUpdateResourceOwnerPasswordParameterValidator, UpdateResourceOwnerPasswordParameterValidator>()
            .AddTransient<IGrantedTokenGeneratorHelper, GrantedTokenGeneratorHelper>()
            .AddTransient<IClientCredentialsGrantTypeParameterValidator, ClientCredentialsGrantTypeParameterValidator>()
            .AddTransient<ITokenActions, TokenActions>()
            .AddTransient<IConsentActions, ConsentActions>()
            .AddTransient<IConfirmConsentAction, ConfirmConsentAction>()
            .AddTransient<IDisplayConsentAction, DisplayConsentAction>()
            .AddTransient<IAuthenticateActions, AuthenticateActions>()
            .AddTransient<IAuthenticateResourceOwnerOpenIdAction, AuthenticateResourceOwnerOpenIdAction>()
            .AddTransient<IAuthenticateHelper, AuthenticateHelper>()
            .AddTransient<IJwtGenerator, JwtGenerator>()
            .AddTransient<IGenerateAuthorizationResponse, GenerateAuthorizationResponse>()
            .AddTransient<ITranslationManager, TranslationManager>()
            .AddTransient<IGenerateAndSendCodeAction, GenerateAndSendCodeAction>()
            .AddTransient<IValidateConfirmationCodeAction, ValidateConfirmationCodeAction>()
            .AddTransient<IRemoveConfirmationCodeAction, RemoveConfirmationCodeAction>()
            .AddTransient<ITwoFactorAuthenticationHandler, TwoFactorAuthenticationHandler>()
            .AddSingleton<IEventPublisher>(options?.EventPublisher ?? new DefaultEventPublisher())
            .AddSingleton<ISubjectBuilder>(options?.SubjectBuilder ?? new DefaultSubjectBuilder())
            .AddSingleton(options?.OAuthConfigurationOptions ?? new OAuthConfigurationOptions())
            .AddSingleton(options?.BasicAuthenticationOptions ?? new BasicAuthenticateOptions())
            .AddSingleton(options?.Scim ?? new ScimOptions { IsEnabled = false })
            .AddSingleton(sp => new DefaultClientRepository(options?.Configuration?.Clients, sp.GetService<HttpClient>(), sp.GetService<IScopeStore>()))
            .AddSingleton(typeof(IClientStore), sp => sp.GetService<DefaultClientRepository>())
            .AddSingleton(typeof(IClientRepository), sp => sp.GetService<DefaultClientRepository>())
            .AddSingleton<IConsentRepository>(new DefaultConsentRepository(options?.Configuration?.Consents))
            .AddSingleton<IProfileRepository>(new DefaultProfileRepository(options?.Configuration?.Profiles))
            .AddSingleton<IResourceOwnerRepository>(new DefaultResourceOwnerRepository(options?.Configuration?.Users))
            .AddSingleton(new DefaultScopeRepository(options?.Configuration?.Scopes))
            .AddSingleton<IScopeRepository>(sp => sp.GetService<DefaultScopeRepository>())
            .AddSingleton<IScopeStore>(sp => sp.GetService<DefaultScopeRepository>())
            .AddSingleton<ITranslationRepository>(new DefaultTranslationRepository(options?.Configuration?.Translations))
            .AddSingleton(options?.Scim ?? new ScimOptions { IsEnabled = false })
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
            .AddTransient<IAuthorizationPolicyValidator, AuthorizationPolicyValidator>()
            .AddTransient<IBasicAuthorizationPolicy, BasicAuthorizationPolicy>()
            .AddTransient<ICustomAuthorizationPolicy, CustomAuthorizationPolicy>()
            .AddTransient<IUmaTokenActions, UmaTokenActions>()
            .AddSingleton(options?.UmaConfigurationOptions ?? new UmaConfigurationOptions())
            .AddSingleton<IPolicyRepository>(new DefaultPolicyRepository(options?.UmaConfigurationOptions?.Policies))
            .AddSingleton<IResourceSetRepository>(new DefaultResourceSetRepository(options?.UmaConfigurationOptions?.ResourceSets))
            .AddSingleton<ITicketStore>(new DefaultTicketStore());
            services.AddDataProtection();
            return s;
        }
    }
}