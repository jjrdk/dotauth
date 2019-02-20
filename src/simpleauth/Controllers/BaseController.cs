namespace SimpleAuth.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;

    public class BaseController : Controller
    {
        protected readonly IAuthenticationService _authenticationService;

        public BaseController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<ClaimsPrincipal> SetUser()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, CookieNames.CookieName).ConfigureAwait(false);
            var isAuthenticed = authenticatedUser?.Identity != null && authenticatedUser.Identity.IsAuthenticated;
            ViewBag.IsAuthenticated = isAuthenticed;
            ViewBag.Name = isAuthenticed ? authenticatedUser.GetName() : "unknown";

            return authenticatedUser;
        }
    }
}
