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
    using Api.Authorization;
    using Api.Discovery;
    using Api.Introspection;
    using Api.Introspection.Actions;
    using Api.Profile.Actions;
    using Api.Token;
    using Api.Token.Actions;
    using Authenticate;
    using Common;
    using Helpers;
    using JwtToken;
    using Microsoft.Extensions.DependencyInjection;
    using Repositories;
    using Services;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Api.Scopes;
    using Api.Scopes.Actions;
    using Microsoft.IdentityModel.Tokens;
    using Translation;
    using Validators;
    using WebSite.Authenticate;
    using WebSite.Authenticate.Actions;
    using WebSite.Authenticate.Common;
    using WebSite.Consent;
    using WebSite.Consent.Actions;
    using WebSite.User.Actions;

    public static class SimpleAuthServerExtensions
    {
        public static IServiceCollection AddSimpleAuth(
            this IServiceCollection serviceCollection,
            OAuthConfigurationOptions configurationOptions = null,
            IReadOnlyCollection<Client> clients = null,
            IReadOnlyCollection<Consent> consents = null,
            IReadOnlyCollection<ResourceOwnerProfile> profiles = null,
            IReadOnlyCollection<ResourceOwner> resourceOwners = null,
            IReadOnlyCollection<Scope> scopes = null,
            IReadOnlyCollection<SimpleAuth.Shared.Models.Translation> translations = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IScopeActions, ScopeActions>();
            serviceCollection.AddTransient<IDeleteScopeOperation, DeleteScopeOperation>();
            serviceCollection.AddTransient<IGetScopeOperation, GetScopeOperation>();
            serviceCollection.AddTransient<IGetScopesOperation, GetScopesOperation>();
            serviceCollection.AddTransient<IUpdateResourceOwnerClaimsParameterValidator, UpdateResourceOwnerClaimsParameterValidator>();
            serviceCollection.AddTransient<IUpdateResourceOwnerPasswordParameterValidator, UpdateResourceOwnerPasswordParameterValidator>();
            serviceCollection.AddTransient<IAddUserOperation, AddUserOperation>();
            serviceCollection.AddTransient<IAddScopeOperation, AddScopeOperation>();
            serviceCollection.AddTransient<IUpdateScopeOperation, UpdateScopeOperation>();
            serviceCollection.AddTransient<ISearchScopesOperation, SearchScopesOperation>();
            serviceCollection.AddTransient<IGrantedTokenGeneratorHelper, GrantedTokenGeneratorHelper>();
            serviceCollection.AddTransient<IConsentHelper, ConsentHelper>();
            serviceCollection.AddTransient<IClientHelper, ClientHelper>();
            serviceCollection.AddTransient<IAuthorizationFlowHelper, AuthorizationFlowHelper>();
            serviceCollection.AddTransient<IClientCredentialsGrantTypeParameterValidator, ClientCredentialsGrantTypeParameterValidator>();
            serviceCollection.AddTransient<IGrantedTokenValidator, GrantedTokenValidator>();
            serviceCollection.AddTransient<IAuthorizationCodeGrantTypeParameterAuthEdpValidator, AuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            //serviceCollection.AddTransient<ICompressor, Compressor>();
            serviceCollection.AddTransient<IParameterParserHelper, ParameterParserHelper>();
            serviceCollection.AddTransient<IAuthorizationActions, AuthorizationActions>();
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
            serviceCollection.AddTransient<IIntrospectionActions, IntrospectionActions>();
            serviceCollection.AddTransient<IPostIntrospectionAction, PostIntrospectionAction>();
            serviceCollection.AddTransient<IIntrospectionParameterValidator, IntrospectionParameterValidator>();
            serviceCollection.AddTransient<IGetConsentsOperation, GetConsentsOperation>();
            serviceCollection.AddTransient<IRemoveConsentOperation, RemoveConsentOperation>();
            serviceCollection.AddTransient<IRevokeTokenAction, RevokeTokenAction>();
            serviceCollection.AddTransient<IGetUserOperation, GetUserOperation>();
            serviceCollection.AddTransient<IUpdateUserCredentialsOperation, UpdateUserCredentialsOperation>();
            serviceCollection.AddTransient<IUpdateUserClaimsOperation, UpdateUserClaimsOperation>();
            serviceCollection.AddTransient<IAddUserOperation, AddUserOperation>();
            serviceCollection.AddTransient<IGenerateAndSendCodeAction, GenerateAndSendCodeAction>();
            serviceCollection.AddTransient<IValidateConfirmationCodeAction, ValidateConfirmationCodeAction>();
            serviceCollection.AddTransient<IRemoveConfirmationCodeAction, RemoveConfirmationCodeAction>();
            serviceCollection.AddTransient<ITwoFactorAuthenticationHandler, TwoFactorAuthenticationHandler>();
            serviceCollection.AddTransient<IGetResourceOwnerClaimsAction, GetResourceOwnerClaimsAction>();
            serviceCollection.AddTransient<IUpdateUserTwoFactorAuthenticatorOperation, UpdateUserTwoFactorAuthenticatorOperation>();
            serviceCollection.AddTransient<IResourceOwnerAuthenticateHelper, ResourceOwnerAuthenticateHelper>();
            serviceCollection.AddTransient<IAmrHelper, AmrHelper>();
            serviceCollection.AddTransient<IRevokeTokenParameterValidator, RevokeTokenParameterValidator>();
            serviceCollection.AddSingleton(configurationOptions ?? new OAuthConfigurationOptions());

            serviceCollection.AddSingleton(sp => new DefaultClientRepository(clients, sp.GetService<HttpClient>(), sp.GetService<IScopeStore>()));
            serviceCollection.AddSingleton(typeof(IClientStore), sp => sp.GetService<DefaultClientRepository>());
            serviceCollection.AddSingleton(typeof(IClientRepository), sp => sp.GetService<DefaultClientRepository>());
            serviceCollection.AddSingleton<IConsentRepository>(new DefaultConsentRepository(consents));
            serviceCollection.AddSingleton<IProfileRepository>(new DefaultProfileRepository(profiles));
            serviceCollection.AddSingleton<IResourceOwnerRepository>(
                new DefaultResourceOwnerRepository(resourceOwners));

            serviceCollection.AddSingleton(new DefaultScopeRepository(scopes));
            serviceCollection.AddSingleton<IScopeRepository>(sp => sp.GetService<DefaultScopeRepository>());
            serviceCollection.AddSingleton<IScopeStore>(sp => sp.GetService<DefaultScopeRepository>());
            serviceCollection.AddSingleton<ITranslationRepository>(new DefaultTranslationRepository(translations));
            return serviceCollection;
        }
    }
}
