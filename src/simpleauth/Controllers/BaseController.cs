namespace SimpleAuth.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Defines the abstract base controller.
    /// </summary>
    /// <seealso cref="Controller" />
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// The authentication service
        /// </summary>
        protected readonly IAuthenticationService _authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        protected BaseController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Sets the user.
        /// </summary>
        /// <returns></returns>
        protected async Task<ClaimsPrincipal> SetUser()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, CookieNames.CookieName).ConfigureAwait(false);
            var isAuthenticed = authenticatedUser?.Identity != null && authenticatedUser.Identity.IsAuthenticated;
            ViewBag.IsAuthenticated = isAuthenticed;
            ViewBag.Name = isAuthenticed ? authenticatedUser.GetName() : "unknown";

            return authenticatedUser;
        }
    }
}
