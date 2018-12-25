using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Host.Extensions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Controllers.Website
{
    using SimpleAuth.Extensions;

    public class BaseController : Controller
    {
        protected readonly IAuthenticationService _authenticationService;

        public BaseController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<ClaimsPrincipal> SetUser()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, HostConstants.CookieNames.CookieName).ConfigureAwait(false);
            var isAuthenticed = authenticatedUser?.Identity != null && authenticatedUser.Identity.IsAuthenticated;
            ViewBag.IsAuthenticated = isAuthenticed;
            if (isAuthenticed)
            {
                ViewBag.Name = authenticatedUser.GetName();
            }
            else
            {
                ViewBag.Name = "unknown";
            }

            return authenticatedUser;
        }
    }
}
