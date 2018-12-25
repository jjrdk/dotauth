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

using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Scim.Core;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Host.Controllers
{
    [Route(ScimConstants.RoutePaths.SchemasController)]
    public class SchemasController : Controller
    {
        [HttpGet("{id}")]
        public Task<IActionResult> Get(string id)
        {
            var result = NotFound();
            return Task.FromResult<IActionResult>(result);
            //return new OkObjectResult(await _schemaStore.GetSchema(id).ConfigureAwait(false));
        }

        [HttpGet]
        public Task<IActionResult> All()
        {
            var result = NotFound();
            return Task.FromResult<IActionResult>(result);
            // return new OkObjectResult(await _schemaStore.GetSchemas().ConfigureAwait(false));
        }
    }
}
