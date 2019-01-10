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

namespace SimpleAuth
{
    using Api.Discovery;
    using Api.Token;
    using Api.Token.Actions;
    using Authenticate;
    using Common;
    using Helpers;
    using JwtToken;
    using Microsoft.Extensions.DependencyInjection;
    using Repositories;
    using Services;
    using Shared.Repositories;
    using System;
    using System.Net.Http;
    using Logging;
    using Shared;
    using Translation;
    using Validators;
    using WebSite.Authenticate;
    using WebSite.Authenticate.Actions;
    using WebSite.Authenticate.Common;
    using WebSite.Consent;
    using WebSite.Consent.Actions;

    internal static class SimpleAuthServerExtensions
    {
        public static IServiceCollection RegisterSimpleAuth(
            this IServiceCollection serviceCollection,
            SimpleAuthOptions options = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IAuthenticateResourceOwnerService, UsernamePasswordAuthenticationService>();

            serviceCollection.AddTransient<IUpdateResourceOwnerClaimsParameterValidator, UpdateResourceOwnerClaimsParameterValidator>();
            serviceCollection.AddTransient<IUpdateResourceOwnerPasswordParameterValidator, UpdateResourceOwnerPasswordParameterValidator>();
            serviceCollection.AddTransient<IGrantedTokenGeneratorHelper, GrantedTokenGeneratorHelper>();
            serviceCollection.AddTransient<IConsentHelper, ConsentHelper>();
            serviceCollection.AddTransient<IClientHelper, ClientHelper>();
            serviceCollection.AddTransient<IAuthorizationFlowHelper, AuthorizationFlowHelper>();
            serviceCollection.AddTransient<IClientCredentialsGrantTypeParameterValidator, ClientCredentialsGrantTypeParameterValidator>();
            serviceCollection.AddTransient<IGrantedTokenValidator, GrantedTokenValidator>();
            serviceCollection.AddTransient<IParameterParserHelper, ParameterParserHelper>();
            serviceCollection.AddTransient<ITokenActions, TokenActions>();
            serviceCollection.AddTransient<IGetTokenByResourceOwnerCredentialsGrantTypeAction, GetTokenByResourceOwnerCredentialsGrantTypeAction>();
            serviceCollection.AddTransient<IGetTokenByAuthorizationCodeGrantTypeAction, GetTokenByAuthorizationCodeGrantTypeAction>();
            serviceCollection.AddTransient<IConsentActions, ConsentActions>();
            serviceCollection.AddTransient<IConfirmConsentAction, ConfirmConsentAction>();
            serviceCollection.AddTransient<IDisplayConsentAction, DisplayConsentAction>();
            serviceCollection.AddTransient<IAuthenticateActions, AuthenticateActions>();
            serviceCollection
                .AddTransient<IAuthenticateResourceOwnerOpenIdAction, AuthenticateResourceOwnerOpenIdAction>();
            serviceCollection.AddTransient<ILocalOpenIdUserAuthenticationAction, LocalOpenIdUserAuthenticationAction>();
            serviceCollection.AddTransient<IAuthenticateHelper, AuthenticateHelper>();
            serviceCollection.AddTransient<IDiscoveryActions, DiscoveryActions>();
            serviceCollection.AddTransient<IJwtGenerator, JwtGenerator>();
            serviceCollection.AddTransient<IGenerateAuthorizationResponse, GenerateAuthorizationResponse>();
            serviceCollection.AddTransient<IAuthenticateClient, AuthenticateClient>();
            serviceCollection
                .AddTransient<IGetTokenByRefreshTokenGrantTypeAction, GetTokenByRefreshTokenGrantTypeAction>();
            serviceCollection.AddTransient<ITranslationManager, TranslationManager>();
            serviceCollection.AddTransient<IGrantedTokenHelper, GrantedTokenHelper>();
            serviceCollection.AddTransient<IRevokeTokenAction, RevokeTokenAction>();
            serviceCollection.AddTransient<IGenerateAndSendCodeAction, GenerateAndSendCodeAction>();
            serviceCollection.AddTransient<IValidateConfirmationCodeAction, ValidateConfirmationCodeAction>();
            serviceCollection.AddTransient<IRemoveConfirmationCodeAction, RemoveConfirmationCodeAction>();
            serviceCollection.AddTransient<ITwoFactorAuthenticationHandler, TwoFactorAuthenticationHandler>();
            serviceCollection.AddTransient<IResourceOwnerAuthenticateHelper, ResourceOwnerAuthenticateHelper>();
            serviceCollection.AddTransient<IAmrHelper, AmrHelper>();
            serviceCollection.AddSingleton<IEventPublisher>(options?.EventPublisher ?? new DefaultEventPublisher());
            serviceCollection.AddSingleton<ISubjectBuilder>(options?.SubjectBuilder ?? new DefaultSubjectBuilder());
            serviceCollection.AddSingleton(options?.OAuthConfigurationOptions ?? new OAuthConfigurationOptions());
            serviceCollection.AddSingleton(options?.BasicAuthenticationOptions ?? new BasicAuthenticateOptions());
            serviceCollection.AddSingleton(options?.Scim ?? new ScimOptions { IsEnabled = false });

            serviceCollection.AddSingleton(sp => new DefaultClientRepository(options?.Configuration?.Clients, sp.GetService<HttpClient>(), sp.GetService<IScopeStore>()));
            serviceCollection.AddSingleton(typeof(IClientStore), sp => sp.GetService<DefaultClientRepository>());
            serviceCollection.AddSingleton(typeof(IClientRepository), sp => sp.GetService<DefaultClientRepository>());
            serviceCollection.AddSingleton<IConsentRepository>(new DefaultConsentRepository(options?.Configuration?.Consents));
            serviceCollection.AddSingleton<IProfileRepository>(new DefaultProfileRepository(options?.Configuration?.Profiles));
            serviceCollection.AddSingleton<IResourceOwnerRepository>(
                new DefaultResourceOwnerRepository(options?.Configuration?.Users));

            serviceCollection.AddSingleton(new DefaultScopeRepository(options?.Configuration?.Scopes));
            serviceCollection.AddSingleton<IScopeRepository>(sp => sp.GetService<DefaultScopeRepository>());
            serviceCollection.AddSingleton<IScopeStore>(sp => sp.GetService<DefaultScopeRepository>());
            serviceCollection.AddSingleton<ITranslationRepository>(new DefaultTranslationRepository(options?.Configuration?.Translations));

            return serviceCollection;
        }
    }
}
