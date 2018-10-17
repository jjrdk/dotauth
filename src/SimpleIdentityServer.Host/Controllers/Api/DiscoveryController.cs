// Copyright 2015 Habart Thierry
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

namespace SimpleIdentityServer.Host.Controllers.Api
{
    using Core.Api.Discovery;
    using Core.Common.DTOs.Responses;
    using Extensions;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Route(Core.Constants.EndPoints.DiscoveryAction)]
    public class DiscoveryController : Controller
    {
        private readonly IDiscoveryActions _discoveryActions;
        private readonly ScimOptions _scim;

        public DiscoveryController(IDiscoveryActions discoveryActions, ScimOptions scim)
        {
            _discoveryActions = discoveryActions;
            _scim = scim;
        }

        [HttpGet]
        public async Task<DiscoveryInformation> Get()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var result = await _discoveryActions.CreateDiscoveryInformation(issuer, _scim.IsEnabled ? _scim.EndPoint : null).ConfigureAwait(false);

            return result;
        }
    }
}
