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

namespace SimpleAuth.Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Requests;
    using Shared.Responses;
    using SimpleAuth;
    using SimpleAuth.Api.Jws;

    [Route(CoreConstants.EndPoints.Jws)]
    public class JwsController : Controller
    {
        private readonly IJwsActions _jwsActions;

        public JwsController(IJwsActions jwsActions)
        {
            _jwsActions = jwsActions;
        }

        [HttpGet]
        public async Task<JwsInformationResponse> GetJws([FromQuery] GetJwsRequest getJwsRequest)
        {
            if (getJwsRequest == null)
            {
                throw new ArgumentNullException(nameof(getJwsRequest));
            }

            var result = await _jwsActions.GetJwsInformation(getJwsRequest.ToParameter()).ConfigureAwait(false);
            return result.ToDto();
        }
        
        [HttpPost]
        public async Task<string> PostJws([FromBody] CreateJwsRequest createJwsRequest)
        {
            if (createJwsRequest == null)
            {
                throw new ArgumentNullException(nameof(createJwsRequest));
            }

            return await _jwsActions.CreateJws(createJwsRequest.ToParameter()).ConfigureAwait(false);
        }
    }
}
