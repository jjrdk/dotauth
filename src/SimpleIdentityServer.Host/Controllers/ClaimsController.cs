namespace SimpleIdentityServer.Host.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Core;
    using Core.Api.Claims;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Requests;
    using Shared.Responses;

    [Route(CoreConstants.EndPoints.Claims)]
    public class ClaimsController : Controller
    {
        //public const string GetClaimsStoreName = "GetClaims";
        //public const string GetClaimStoreName = "GetClaim_";
        private readonly IClaimActions _claimActions;

        public ClaimsController(IClaimActions claimActions)
        {
            _claimActions = claimActions;
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            //if (!await _representationManager.CheckRepresentationExistsAsync(this, GetClaimStoreName + id))
            //{
            //    return new ContentResult
            //    {
            //        StatusCode = 412
            //    };
            //}

            var result = await _claimActions.Get(id).ConfigureAwait(false);
            if (result == null)
            {
                return new NotFoundResult();
            }

            var response = result.ToDto();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClaimStoreName + id);
            return new OkObjectResult(response);
        }

        [HttpGet]
        [Authorize("manager")]
        public async Task<IActionResult> GetAll()
        {
            //if (!await _representationManager.CheckRepresentationExistsAsync(this, GetClaimsStoreName))
            //{
            //    return new ContentResult
            //    {
            //        StatusCode = 412
            //    };
            //}

            var result = (await _claimActions.GetAll().ConfigureAwait(false)).ToDtos();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClaimsStoreName);
            return new OkObjectResult(result);
        }

        [HttpPost(".search")]
        [Authorize("manager")]
        public async Task<IActionResult> Search([FromBody] SearchClaimsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var parameter = request.ToParameter();
            var result = await _claimActions.Search(parameter).ConfigureAwait(false);
            return new OkObjectResult(result.ToDto());
        }
        
        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!await _claimActions.Delete(id).ConfigureAwait(false))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClaimStoreName + id, false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClaimsStoreName, false);
            return new NoContentResult();
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<IActionResult> Add([FromBody] ClaimResponse claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var result = await _claimActions.Add(claim.ToParameter()).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClaimStoreName, false);
            return new OkObjectResult(result);
        }
    }
}
