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

namespace SimpleIdentityServer.Core
{
    using Api.Claims;
    using Api.Claims.Actions;
    using Api.Jwe;
    using Api.Jwe.Actions;
    using Api.Jws;
    using Api.Jws.Actions;
    using Api.Scopes;
    using Api.Scopes.Actions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Validators;
    using WebSite.User.Actions;

    public static class AddSimpleIdentityServerManagerCoreExtensions
    {
        public static IServiceCollection AddSimpleIdentityServerManagerCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IJwsActions, JwsActions>();
            serviceCollection.AddTransient<IGetJwsInformationAction, GetJwsInformationAction>();
            serviceCollection.AddTransient<IJweActions, JweActions>();
            serviceCollection.AddTransient<IGetJweInformationAction, GetJweInformationAction>();
            serviceCollection.AddTransient<ICreateJweAction, CreateJweAction>();
            serviceCollection.AddTransient<ICreateJwsAction, CreateJwsAction>();
            serviceCollection.AddTransient<IJsonWebKeyHelper, JsonWebKeyHelper>();
            serviceCollection.AddTransient<IJsonWebKeyEnricher, JsonWebKeyEnricher>();
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
            serviceCollection.AddTransient<IClaimActions, ClaimActions>();
            serviceCollection.AddTransient<IAddClaimAction, AddClaimAction>();
            serviceCollection.AddTransient<IDeleteClaimAction, DeleteClaimAction>();
            serviceCollection.AddTransient<IGetClaimAction, GetClaimAction>();
            serviceCollection.AddTransient<ISearchClaimsAction, SearchClaimsAction>();
            serviceCollection.AddTransient<IGetClaimsAction, GetClaimsAction>();
            return serviceCollection;
        }
    }
}
