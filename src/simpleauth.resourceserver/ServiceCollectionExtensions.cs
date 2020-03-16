// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.ResourceServer
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleAuth.Client;

    /// <summary>
    /// Defines extensions to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds UMA dependencies to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add dependencies to.</param>
        /// <param name="umaAuthority">The <see cref="Uri"/> where to find the discovery document.</param>
        /// <returns></returns>
        public static IServiceCollection AddUmaClient(this IServiceCollection serviceCollection, Uri umaAuthority)
        {
            serviceCollection.AddSingleton(sp => new UmaClient(sp.GetRequiredService<HttpClient>(), umaAuthority));
            serviceCollection.AddTransient<IUmaPermissionClient, UmaClient>(sp => sp.GetRequiredService<UmaClient>());
            serviceCollection.AddTransient<IPolicyClient, UmaClient>(sp => sp.GetRequiredService<UmaClient>());

            return serviceCollection;
        }
    }
}
