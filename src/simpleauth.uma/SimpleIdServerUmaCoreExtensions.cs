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

namespace SimpleAuth.Uma
{
    using Api.ConfigurationController;
    using Api.ConfigurationController.Actions;
    using Api.PermissionController;
    using Api.PermissionController.Actions;
    using Api.PolicyController;
    using Api.PolicyController.Actions;
    using Api.ResourceSetController;
    using Api.ResourceSetController.Actions;
    using Api.Token;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using Policies;
    using Repositories;
    using SimpleAuth;
    using Stores;
    using System.Collections.Generic;
    using SimpleAuth.Shared.Repositories;
    using Validators;

    public static class SimpleIdServerUmaCoreExtensions
    {
        public static IServiceCollection AddSimpleIdServerUmaCore(this IServiceCollection serviceCollection, UmaConfigurationOptions umaConfigurationOptions = null, IReadOnlyCollection<ResourceSet> resources = null, IReadOnlyCollection<Policy> policies = null)
        {
            serviceCollection.AddTransient<IResourceSetActions, ResourceSetActions>();
            serviceCollection.AddTransient<IAddResourceSetAction, AddResourceSetAction>();
            serviceCollection.AddTransient<IGetResourceSetAction, GetResourceSetAction>();
            serviceCollection.AddTransient<IUpdateResourceSetAction, UpdateResourceSetAction>();
            serviceCollection.AddTransient<IDeleteResourceSetAction, DeleteResourceSetAction>();
            serviceCollection.AddTransient<IGetAllResourceSetAction, GetAllResourceSetAction>();
            serviceCollection.AddTransient<IResourceSetParameterValidator, ResourceSetParameterValidator>();
            serviceCollection.AddTransient<IPermissionControllerActions, PermissionControllerActions>();
            serviceCollection.AddTransient<IAddPermissionAction, AddPermissionAction>();
            serviceCollection.AddTransient<IRepositoryExceptionHelper, RepositoryExceptionHelper>();
            serviceCollection.AddTransient<IAuthorizationPolicyValidator, AuthorizationPolicyValidator>();
            serviceCollection.AddTransient<IBasicAuthorizationPolicy, BasicAuthorizationPolicy>();
            serviceCollection.AddTransient<ICustomAuthorizationPolicy, CustomAuthorizationPolicy>();
            serviceCollection.AddTransient<IAddAuthorizationPolicyAction, AddAuthorizationPolicyAction>();
            serviceCollection.AddTransient<IPolicyActions, PolicyActions>();
            serviceCollection.AddTransient<IGetAuthorizationPolicyAction, GetAuthorizationPolicyAction>();
            serviceCollection.AddTransient<IDeleteAuthorizationPolicyAction, DeleteAuthorizationPolicyAction>();
            serviceCollection.AddTransient<IGetAuthorizationPoliciesAction, GetAuthorizationPoliciesAction>();
            serviceCollection.AddTransient<IUpdatePolicyAction, UpdatePolicyAction>();
            serviceCollection.AddTransient<IConfigurationActions, ConfigurationActions>();
            serviceCollection.AddTransient<IGetConfigurationAction, GetConfigurationAction>();
            serviceCollection.AddTransient<IAddResourceSetToPolicyAction, AddResourceSetToPolicyAction>();
            serviceCollection.AddTransient<IDeleteResourcePolicyAction, DeleteResourcePolicyAction>();
            serviceCollection.AddTransient<IGetPoliciesAction, GetPoliciesAction>();
            serviceCollection.AddTransient<ISearchAuthPoliciesAction, SearchAuthPoliciesAction>();
            serviceCollection.AddTransient<ISearchResourceSetOperation, SearchResourceSetOperation>();
            serviceCollection.AddTransient<IUmaTokenActions, UmaTokenActions>();
            serviceCollection.AddSingleton(umaConfigurationOptions);
            serviceCollection.AddSingleton<IPolicyRepository>(new DefaultPolicyRepository(policies));
            serviceCollection.AddSingleton<IResourceSetRepository>(new DefaultResourceSetRepository(resources));
            serviceCollection.AddSingleton<ITicketStore>(new DefaultTicketStore());

            return serviceCollection;
        }
    }
}
