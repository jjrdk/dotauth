namespace SimpleAuth.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.ViewModels;

    /// <summary>
    /// Defines the account controller
    /// </summary>
    [Route("Account")]
    public class AccountController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authenticationService"></param>
        public AccountController(IAuthenticationService authenticationService)
            : base(authenticationService)
        {
        }

        /// <summary>
        /// Handles the default request.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser is { Identity: { IsAuthenticated: true } })
            {
                return RedirectToAction("Index", "User");
            }

            return RedirectToAction("Index", "Authenticate"); //View();
        }

        /// <summary>
        /// Handles the AccessDenied request.
        /// </summary>
        /// <returns></returns>
        [HttpGet("AccessDenied")]
        public async Task<IActionResult> AccessDenied()
        {
            return Request.Query.TryGetValue("ReturnUrl", out var returnUrl)
                ? RedirectToAction("Index", "Authenticate", new {ReturnUrl = returnUrl})
                : RedirectToAction("Index", "Authenticate");
        }

        /// <summary>
        /// Handles the update account request.
        /// </summary>
        /// <param name="updateResourceOwnerViewModel">The update view model.</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Index(UpdateResourceOwnerViewModel? updateResourceOwnerViewModel)
        {
            if (updateResourceOwnerViewModel == null)
            {
                return BadRequest();
            }

            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser is { Identity: { IsAuthenticated: true } })
            {
                return RedirectToAction("Index", "User");
            }

            //await _resourceOwnerRepository.AddResourceOwner(new AddUserParameter
            //{
            //    Login = updateResourceOwnerViewModel.Login,
            //    Password = updateResourceOwnerViewModel.Password
            //});

            return RedirectToAction("Index", "Authenticate");
        }
    }
}