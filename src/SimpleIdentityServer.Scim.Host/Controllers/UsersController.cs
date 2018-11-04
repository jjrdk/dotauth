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

namespace SimpleIdentityServer.Scim.Host.Controllers
{
    using Core.EF;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleIdentityServer.Core.Common;
    using SimpleIdentityServer.Core.Common.DTOs;
    using SimpleIdentityServer.Core.Common.Models;
    using SimpleIdentityServer.Core.Common.Repositories;
    using SimpleIdentityServer.Core.Services;
    using SimpleIdentityServer.Core.WebSite.User.Actions;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    [Route(Core.Constants.RoutePaths.UsersController)]
    public class UsersController : Controller
    {
        private readonly IAddUserOperation _addUserOperation;
        private readonly IResourceOwnerRepository _userStore;
        private readonly ISubjectBuilder _subjectBuilder;

        public UsersController(
            IAddUserOperation addUserOperation,
            IResourceOwnerRepository userStore,
            ISubjectBuilder subjectBuilder)
        {
            _addUserOperation = addUserOperation;
            _userStore = userStore;
            _subjectBuilder = subjectBuilder;
        }

        [Authorize(ScimConstants.ScimPolicies.ScimManage)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ScimUser scimUser, CancellationToken cancellationToken)
        {
            if (scimUser == null)
            {
                throw new ArgumentNullException(nameof(scimUser));
            }

            var existing = await _userStore
                .Get(x => x.UserProfile.Id == scimUser.Id, cancellationToken)
                .ConfigureAwait(false);

            if (existing.Any())
            {
                return StatusCode((int)HttpStatusCode.ExpectationFailed);
            }

            if (string.IsNullOrWhiteSpace(scimUser.Id))
            {
                scimUser.Id = Guid.NewGuid().ToString("N");
            }
            var id = await _subjectBuilder.BuildSubject(User.Claims.ToArray(), scimUser).ConfigureAwait(false);
            var pwd = Guid.NewGuid().ToString("N");
            var ro = new ResourceOwner { Id = id, Password = pwd, ScimOnly = true, UserProfile = scimUser };

            //var ro = new ResourceOwner
            //{
            //    ScimOnly = true,
            //    CreateDateTime = DateTime.UtcNow,
            //    UserProfile = scimUser
            //};
            var result = await _addUserOperation.Execute(ro).ConfigureAwait(false);
            //Response.Headers[HttpResponseHeader.Location.ToString()] = ;
            var location = string.Format(
                Request.GetAbsoluteUriWithVirtualPath() + Core.Constants.RoutePaths.UsersController + "/{0}",
                scimUser.UserName);
            return result
                ? (IActionResult)Created(location, scimUser)
                : StatusCode((int)HttpStatusCode.BadRequest);
        }

        [Authorize(ScimConstants.ScimPolicies.ScimRead)]
        [HttpGet("{id}")]
        public Task<IActionResult> Get(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? Task.FromResult<IActionResult>(new BadRequestResult()) : GetUser(id);
        }

        //[Authorize(ScimConstants.ScimPolicies.ScimManage)]
        //[HttpPatch("{id}")]
        //public Task<ActionResult> Patch(string id, [FromBody] JObject jObj)
        //{
        //    if (string.IsNullOrWhiteSpace(id))
        //    {
        //        throw new ArgumentNullException(nameof(id));
        //    }

        //    if (jObj == null)
        //    {
        //        throw new ArgumentNullException(nameof(jObj));
        //    }

        //    return PatchUser(id, jObj);
        //}

        [Authorize(ScimConstants.ScimPolicies.ScimManage)]
        [HttpPut("{id}")]
        public Task<IActionResult> Update(string id, [FromBody] ScimUser jObj, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return UpdateUser(id, jObj, cancellationToken);
        }

        [Authorize(ScimConstants.ScimPolicies.ScimManage)]
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return DeleteUser(id);
        }

        [Authorize(ScimConstants.ScimPolicies.ScimRead)]
        [HttpGet]
        public async Task<IActionResult> SearchUsers()
        {
            //var result = await _usersAction.SearchUsers(Request.Query, GetLocationPattern()).ConfigureAwait(false);
            var result = (await _userStore.Get((Expression<Func<ResourceOwner, bool>>)null).ConfigureAwait(false))
                .Select(x => x.UserProfile)
                .ToArray();

            return result.Length > 0
                ? (IActionResult)new OkObjectResult(result)
                : new StatusCodeResult((int)HttpStatusCode.NotFound);
        }

        [Authorize(ScimConstants.ScimPolicies.ScimRead)]
        [HttpPost(".search")]
        public async Task<IActionResult> SearchUsers([FromBody] SearchParameter filter)
        {
            //var result = await _usersAction.SearchUsers(scimUser, GetLocationPattern()).ConfigureAwait(false);
            var result = (await _userStore.Get(x => true).ConfigureAwait(false))
                .Skip(filter.StartIndex)
                .Take(filter.Count)
                .Select(x => x.UserProfile)
                .ToArray();

            return result.Length > 0
                ? (IActionResult)new OkObjectResult(result)
                : new StatusCodeResult((int)HttpStatusCode.NotFound);
        }

        [HttpPost("Me")]
        [Authorize("authenticated")]
        public async Task<IActionResult> CreateAuthenticatedUser([FromBody] ScimUser scimUser)
        {
            var subject = GetSubject(User);
            if (string.IsNullOrWhiteSpace(subject))
            {
                return GetMissingSubjectError();
            }

            var id = await _subjectBuilder.BuildSubject(User.Claims.ToArray(), scimUser).ConfigureAwait(false);
            var pwd = Guid.NewGuid().ToString("N");
            var ro = new ResourceOwner { Id = id, Password = pwd, ScimOnly = true, UserProfile = scimUser };
            var result = await _addUserOperation.Execute(ro).ConfigureAwait(false);
            var location = string.Format(
                Request.GetAbsoluteUriWithVirtualPath() + Core.Constants.RoutePaths.UsersController + "/{0}",
                scimUser.UserName);
            return result ? Created(location, ro.UserProfile) : (IActionResult)StatusCode((int)HttpStatusCode.BadRequest);
        }

        [HttpGet("Me")]
        [Authorize("authenticated")]
        public Task<IActionResult> GetAuthenticateUser()
        {
            var scimId = GetScimIdentifier(User);
            return string.IsNullOrWhiteSpace(scimId)
                ? Task.FromResult<IActionResult>(GetMissingScimIdentifierError())
                : GetUser(scimId);
        }

        //[HttpPatch("Me")]
        //[Authorize("authenticated")]
        //public Task<ActionResult> PatchAuthenticatedUser([FromBody] JObject scimUser)
        //{
        //    var scimId = GetScimIdentifier(User);
        //    if (string.IsNullOrWhiteSpace(scimId))
        //    {
        //        return Task.FromResult(GetMissingScimIdentifierError());
        //    }

        //    return PatchUser(scimId, scimUser);
        //}

        [HttpPut("Me")]
        [Authorize("authenticated")]
        public Task<IActionResult> UpdateAuthenticatedUser([FromBody] ScimUser scimUser, CancellationToken cancellationToken)
        {
            var scimId = GetScimIdentifier(User);
            if (string.IsNullOrWhiteSpace(scimId))
            {
                return Task.FromResult<IActionResult>(GetMissingScimIdentifierError());
            }

            return UpdateUser(scimId, scimUser, cancellationToken);
        }

        [HttpDelete("Me")]
        [Authorize("authenticated")]
        public Task<ActionResult> DeleteAuthenticatedUser()
        {
            var scimId = GetScimIdentifier(User);
            if (string.IsNullOrWhiteSpace(scimId))
            {
                return Task.FromResult(GetMissingScimIdentifierError());
            }

            return DeleteUser(scimId);
        }

        //private async Task<ActionResult> CreateUser(ScimUser scimUser)
        //{
        //    var result = await _addUserOperation.Execute(scimUser).ConfigureAwait(false);

        //    return this.GetActionResult(result);
        //}

        private async Task<IActionResult> GetUser(string id)
        {
            var result = (await _userStore.Get(x => x.UserProfile.Id == id).ConfigureAwait(false)).FirstOrDefault();

            return result == null
                ? (IActionResult)new StatusCodeResult((int)HttpStatusCode.NotFound)
                : new OkObjectResult(result.UserProfile);
        }

        //private async Task<ActionResult> PatchUser(string id, JObject scimUser)
        //{
        //    var result = await _usersAction.PatchUser(id, scimUser, GetLocationPattern()).ConfigureAwait(false);
        //    //if (result.IsSucceed())
        //    //{
        //    //    await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(UsersName, result.Id), result.Version, true);
        //    //}

        //    return this.GetActionResult(result);
        //}

        private async Task<IActionResult> UpdateUser(string id, ScimUser scimUser, CancellationToken cancellationToken)
        {
            var user = (await _userStore.Get(x => x.UserProfile.Id == id, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
            if (user == null)
            {
                return BadRequest();
            }
            user.UserProfile = scimUser;
            //_userStore.UpdateAsync()
            //_usersAction.UpdateUser(id, scimUser, GetLocationPattern()).ConfigureAwait(false);

            return new OkObjectResult(user);
        }

        private async Task<ActionResult> DeleteUser(string id)
        {
            var result = await _userStore.DeleteProfile(id).ConfigureAwait(false);

            return result ? NoContent() : new StatusCodeResult((int)HttpStatusCode.NotFound);
        }

        //private Uri GetLocationPattern()
        //{
        //    return new Uri(
        //        new Uri(Request.GetAbsoluteUriWithVirtualPath()),
        //        Core.JwtConstants.RoutePaths.UsersController);

        //    //+"/{id}";
        //}

        private static string GetSubject(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal?.Identity == null || !claimsPrincipal.Identity.IsAuthenticated || claimsPrincipal.Claims == null)
            {
                return null;
            }

            var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "sub");

            return claim?.Value;
        }

        private static string GetScimIdentifier(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal?.Identity == null || !claimsPrincipal.Identity.IsAuthenticated || claimsPrincipal.Claims == null)
            {
                return null;
            }

            var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "scim_id");

            return claim?.Value;
        }

        private static ActionResult GetMissingSubjectError()
        {
            var error = new ScimErrorResponse
            {
                Detail = "the subject is missing",
                Schemas = new[] { ScimConstants.Messages.Error },
                Status = (int)HttpStatusCode.BadRequest
            };
            return new JsonResult(error)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        private static ActionResult GetMissingScimIdentifierError()
        {
            var error = new ScimErrorResponse
            {
                Detail = "the scim_id claim is missing",
                Schemas = new[] { ScimConstants.Messages.Error },
                Status = (int)HttpStatusCode.BadRequest
            };
            return new JsonResult(error)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }
    }
}
