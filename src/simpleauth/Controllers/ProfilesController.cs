namespace SimpleAuth.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Errors;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Repositories;
    using Shared.Requests;
    using Shared.Responses;

    [Route("profiles")]
    public class ProfilesController : Controller
    {
        private readonly GetUserProfilesAction _getUserProfiles;
        private readonly LinkProfileAction _linkProfile;
        private readonly UnlinkProfileAction _unlinkProfile;

        public ProfilesController(
            IResourceOwnerRepository resourceOwnerRepository,
            IProfileRepository profileRepository)
        {
            _getUserProfiles = new GetUserProfilesAction(resourceOwnerRepository, profileRepository);
            _linkProfile = new LinkProfileAction(resourceOwnerRepository, profileRepository);
            _unlinkProfile = new UnlinkProfileAction(resourceOwnerRepository, profileRepository);
        }

        [HttpGet(".me")]
        [Authorize("connected_user")]
        public async Task<IActionResult> GetProfiles()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }

            var subject = User.GetSubject();
            return await GetProfiles(subject).ConfigureAwait(false);
        }

        [HttpGet("{subject}")]
        [Authorize("manage_profile")]
        public async Task<IActionResult> GetProfiles(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return BuildMissingParameter(nameof(subject));
            }

            var profiles = await _getUserProfiles.Execute(subject).ConfigureAwait(false);
            return new OkObjectResult(profiles.Select(p => p.ToDto()));
        }

        [HttpPost(".me")]
        [Authorize("connected_user")]
        public Task<IActionResult> AddProfile([FromBody] LinkProfileRequest linkProfileRequest)
        {
            return AddProfile(User.GetSubject(), linkProfileRequest);
        }

        [HttpPost("{subject}")]
        [Authorize("manage_profile")]
        public async Task<IActionResult> AddProfile(string subject, [FromBody] LinkProfileRequest linkProfileRequest)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return BuildMissingParameter(nameof(subject));
            }

            if (linkProfileRequest == null)
            {
                return BuildMissingParameter(nameof(linkProfileRequest));
            }

            if (string.IsNullOrWhiteSpace(linkProfileRequest.UserId))
            {
                return BuildMissingParameter(nameof(linkProfileRequest.UserId));
            }

            if (string.IsNullOrWhiteSpace(linkProfileRequest.Issuer))
            {
                return BuildMissingParameter(nameof(linkProfileRequest.Issuer));
            }

            await _linkProfile.Execute(subject, linkProfileRequest.UserId, linkProfileRequest.Issuer, linkProfileRequest.Force).ConfigureAwait(false);
            return new NoContentResult();
        }

        [HttpDelete(".me/{externalId}")]
        [Authorize("connected_user")]
        public Task<IActionResult> RemoveProfile(string externalId)
        {
            return RemoveProfile(User.GetSubject(), externalId);
        }

        [HttpDelete("{subject}/{externalId}")]
        [Authorize("manage_profile")]
        public async Task<IActionResult> RemoveProfile(string subject, string externalId)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return BuildMissingParameter(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(externalId))
            {
                return BuildMissingParameter(nameof(externalId));
            }

            await _unlinkProfile.Execute(subject, externalId).ConfigureAwait(false);
            return new NoContentResult();
        }

        private static IActionResult BuildMissingParameter(string parameterName)
        {
            var error = new ErrorResponse
            {
                Error = ErrorCodes.InvalidRequestCode,
                ErrorDescription = string.Format(ErrorDescriptions.MissingParameter, parameterName)
            };

            return new JsonResult(error)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }
    }
}
