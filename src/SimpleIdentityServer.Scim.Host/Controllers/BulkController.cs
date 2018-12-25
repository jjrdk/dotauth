// Copyright 2017 Habart Thierry
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core;
using SimpleIdentityServer.Scim.Host.Extensions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Host.Controllers
{
    using Core.Results;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using ScimConstants = Core.ScimConstants;

    [Route(ScimConstants.RoutePaths.BulkController)]
    public class BulkController : Controller
    {
        private readonly BulkAction _bulkAction = new BulkAction();

        [Authorize(SimpleAuth.Shared.ScimConstants.ScimPolicies.ScimManage)]
        [HttpPost]
        public Task<IActionResult> Post([FromBody] BulkRequest bulk)
        {
            if (bulk == null)
            {
                throw new ArgumentNullException(nameof(bulk));
            }

            return Task.FromResult(_bulkAction.Execute(bulk, GetLocationPattern()));
        }

        private string GetLocationPattern()
        {
            return Request.GetAbsoluteUriWithVirtualPath() + "/{rootPath}";
        }


        internal class BulkAction
        {
            public IActionResult Execute(BulkRequest bulk, string baseUrl)
            {
                // 1. Check parameter.
                if (bulk == null)
                {
                    throw new ArgumentNullException(nameof(bulk));
                }

                // 3. Execute bulk operation.
                var numberOfErrors = 0;
                var operationsResult = new JArray();
                foreach (var operation in bulk.Operations)
                {
                    ApiActionResult operationResult = null;
                    //if (operation.Method == HttpMethod.Post)
                    //{
                    //    operationResult = await _addRepresentationAction.Execute(operation.Data, operation.LocationPattern, operation.SchemaId, operation.ResourceType).ConfigureAwait(false);
                    //}
                    //else if (operation.Method == HttpMethod.Put)
                    //{
                    //    operationResult = await _updateRepresentationAction.Execute(operation.ResourceId, operation.Data, operation.SchemaId, operation.LocationPattern, operation.ResourceType).ConfigureAwait(false);
                    //}
                    //else if (operation.Method == HttpMethod.Delete)
                    //{
                    //    operationResult = await _deleteRepresentationAction.Execute(operation.ResourceId).ConfigureAwait(false);
                    //}
                    //else if (operation.Method.Method == "PATCH")
                    //{
                    //    operationResult = await _patchRepresentationAction.Execute(operation.ResourceId, operation.Data, operation.SchemaId, operation.LocationPattern).ConfigureAwait(false);
                    //}

                    // 3.2. If maximum number of errors has been reached then return an error.
                    if (!operationResult.IsSucceed())
                    {
                        numberOfErrors++;
                        //if (bulk.BulkResult.FailOnErrors.HasValue && numberOfErrors > bulk.BulkResult.FailOnErrors)
                        //{
                        //    return _apiResponseFactory.CreateError(HttpStatusCode.InternalServerError,
                        //        _errorResponseFactory.CreateError(
                        //            string.Format(ErrorMessages.TheMaximumNumberOfErrorHasBeenReached, bulk.BulkResult.FailOnErrors),
                        //            HttpStatusCode.InternalServerError,
                        //            ScimConstants.ScimTypeValues.TooMany));
                        //}
                    }

                    operationsResult.Add(CreateOperationResponse(operationResult, operation));
                }

                var response = CreateResponse(operationsResult);
                return new OkObjectResult(response);
                //_apiResponseFactory.CreateResultWithContent(HttpStatusCode.OK, response);
            }

            private JObject CreateResponse(JArray operationsResult)
            {
                var schemas = new JArray { SimpleAuth.Shared.ScimConstants.Messages.BulkResponse };
                var result = new JObject
                {
                    {SimpleAuth.Shared.ScimConstants.ScimResourceNames.Schemas, schemas},
                    {SimpleAuth.Shared.ScimConstants.PatchOperationsRequestNames.Operations, operationsResult}
                };
                return result;
            }

            private BulkOperationResponse CreateOperationResponse(ApiActionResult apiActionResult, BulkOperationRequest bulkOperation)
            {
                var response = new BulkOperationResponse
                {
                    Method = bulkOperation.Method,
                    Status = apiActionResult.StatusCode
                };

                if (!string.IsNullOrWhiteSpace(bulkOperation.BulkId))
                {
                    response.BulkId = bulkOperation.BulkId;
                }

                if (!string.IsNullOrWhiteSpace(bulkOperation.Version))
                {
                    response.Version = bulkOperation.Version;
                }

                if (!string.IsNullOrWhiteSpace(bulkOperation.Path))
                {
                    response.Path = bulkOperation.Path;
                }

                if (!string.IsNullOrWhiteSpace(apiActionResult.Location))
                {
                    response.Location = apiActionResult.Location;
                }

                if (apiActionResult.Content != null)
                {
                    response.Response = JObject.FromObject(apiActionResult.Content);
                }

                return response;
            }
        }
    }
}
