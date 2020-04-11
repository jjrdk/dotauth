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

namespace SimpleAuth.Controllers
{
    using Api.Discovery;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Repositories;
    using Shared.Responses;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Filters;

    /// <summary>
    /// Defines the discovery controller
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route(CoreConstants.EndPoints.DiscoveryAction)]
    [ThrottleFilter]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, NoStore = false)]
    public class DiscoveryController : ControllerBase
    {
        private readonly DiscoveryActions _discoveryActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryController"/> class.
        /// </summary>
        /// <param name="scopeRepository">The scope repository.</param>
        public DiscoveryController(IScopeRepository scopeRepository)
        {
            _discoveryActions = new DiscoveryActions(scopeRepository);
        }

        /// <summary>
        /// Handles the default GET request..
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<DiscoveryInformation> Get(CancellationToken cancellationToken)
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var result = await _discoveryActions.CreateDiscoveryInformation(issuer, cancellationToken).ConfigureAwait(false);

            return result;
        }
    }
}
