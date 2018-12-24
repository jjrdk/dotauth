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

namespace SimpleIdentityServer.Host.Controllers.Api
{
    using System.Threading.Tasks;
    using Core.Api.Jwks;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Requests;

    [Route(Core.CoreConstants.EndPoints.Jwks)]
    public class JwksController : Controller
    {
        private readonly IJwksActions _jwksActions;

        public JwksController(IJwksActions jwksActions)
        {
            _jwksActions = jwksActions;
        }

        [HttpGet]
        public async Task<JsonWebKeySet> Get()
        {
            return await _jwksActions.GetJwks().ConfigureAwait(false);
        }

        [HttpPut]
        public async Task<bool> Put()
        {
            return await _jwksActions.RotateJwks().ConfigureAwait(false);
        }
    }
}
