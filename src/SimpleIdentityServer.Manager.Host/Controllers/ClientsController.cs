#region copyright
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
#endregion

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Core.Api.Clients;
using SimpleIdentityServer.Manager.Host.Extensions;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.Clients)]
    public class ClientsController : Controller
    {
        //public const string GetClientsStoreName = "GetClients";
        //public const string GetClientStoreName = "GetClient_";
        private readonly IClientActions _clientActions;
        //private readonly IRepresentationManager _representationManager;

        public ClientsController(IClientActions clientActions)
        {
            _clientActions = clientActions;
        }
        
        [HttpGet]
        [Authorize("manager")]
        public async Task<ActionResult> GetAll()
        {
            //if (!await _representationManager.CheckRepresentationExistsAsync(this, GetClientsStoreName))
            //{
            //    return new ContentResult
            //    {
            //        StatusCode = 412
            //    };
            //}

            var result =  (await _clientActions.GetClients().ConfigureAwait(false)).ToDtos();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName);
            return new OkObjectResult(result);
        }

        [HttpPost(".search")]
        [Authorize("manager")]
        public async Task<ActionResult> Search([FromBody] SearchClientsRequest request)
        {
            if (request == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameter = request.ToSearchClientParameter();
            var result = await _clientActions.Search(parameter).ConfigureAwait(false);
            return new OkObjectResult(result.ToDto());
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "identifier is missing", HttpStatusCode.BadRequest);
            }

            //if (!await _representationManager.CheckRepresentationExistsAsync(this, GetClientStoreName + id))
            //{
            //    return new ContentResult
            //    {
            //        StatusCode = 412
            //    };
            //}

            var result = await _clientActions.GetClient(id).ConfigureAwait(false);
            if (result == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "client doesn't exist", HttpStatusCode.NotFound);
            }

            var response = result.ToDto();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientStoreName + id);
            return new OkObjectResult(response);
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "identifier is missing", HttpStatusCode.BadRequest);
            }

            if (!await _clientActions.DeleteClient(id).ConfigureAwait(false))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientStoreName + id, false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName, false);
            return new NoContentResult();
        }

        [HttpPut]
        [Authorize("manager")]
        public async Task<ActionResult> Put([FromBody] UpdateClientRequest updateClientRequest)
        {
            if (updateClientRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            if (!await _clientActions.UpdateClient(updateClientRequest.ToParameter()).ConfigureAwait(false))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientStoreName + updateClientRequest.ClientId, false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName, false);
            return new NoContentResult();
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<ActionResult> Add([FromBody] AddClientRequest client)
        {
            if (client == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var result = await _clientActions.AddClient(client.ToParameter()).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName, false);
            return new OkObjectResult(result);
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new SimpleIdentityServer.Common.Dtos.Responses.ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
