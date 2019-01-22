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

namespace SimpleAuth.Client
{
    using System;
    using System.Threading.Tasks;
    using Operations;
    using Shared.Responses;

    internal class DiscoveryClient : IDiscoveryClient
    {
        private readonly IGetDiscoveryOperation _getDiscoveryOperation;

        public DiscoveryClient(IGetDiscoveryOperation getDiscoveryOperation)
        {
            _getDiscoveryOperation = getDiscoveryOperation;
        }

        /// <summary>
        /// Get information about open-id contract asynchronously.
        /// </summary>
        /// <param name="discoveryDocumentationUri">Absolute URI of the open-id contract</param>
        /// <exception cref="ArgumentNullException">Thrown when parameter is null</exception>
        /// <returns>Open-id contract</returns>
        public Task<DiscoveryInformation> GetDiscoveryInformationAsync(Uri discoveryDocumentationUri)
        {
            if (discoveryDocumentationUri == null)
            {
                throw new ArgumentNullException(nameof(discoveryDocumentationUri));
            }
            
            return _getDiscoveryOperation.ExecuteAsync(discoveryDocumentationUri);
        }
    }
}
