namespace SimpleAuth.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using SimpleAuth.Shared;
    using SimpleAuth.ViewModels;

    /// <summary>
    /// Defines the account controller
    /// </summary>
    [Route("Account")]
    public class AccountController : BaseController
    {
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelper _urlHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authenticationService"></param>
        /// <param name="authenticationSchemeProvider"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="urlHelperFactory"></param>
        public AccountController(
            IAuthenticationService authenticationService,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory)
            : base(authenticationService)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _actionContextAccessor = actionContextAccessor;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
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
        public async Task AccessDenied()
        {
            Request.Query.TryGetValue("ReturnUrl", out var returnUrl);
            //if (!Request.Query.TryGetValue("ReturnUrl", out var returnUrl))
            //{
            //    return RedirectToAction("Index", "Authenticate");
            //}

            var scheme = await _authenticationSchemeProvider.GetDefaultChallengeSchemeAsync().ConfigureAwait(false);
            var values = string.IsNullOrWhiteSpace(returnUrl) ? null : new { ReturnUrl = returnUrl };
            var redirectUrl = _urlHelper.Action("LoginCallback", "Authenticate", values, Request.Scheme);
            //if (!User.IsAuthenticated())
            //{
            await _authenticationService.ChallengeAsync(
                       Request.HttpContext,
                       scheme!.Name,
                       new AuthenticationProperties { RedirectUri = redirectUrl })
                   .ConfigureAwait(false);
            //    //return RedirectToAction("Index", "Authenticate", values);
            //}
            //else
            //{

            //    await _authenticationService.SignInAsync(
            //            Request.HttpContext,
            //            scheme!.Name,
            //            User,
            //            new AuthenticationProperties {RedirectUri = redirectUrl})
            //        .ConfigureAwait(false);
            //}
            //return RedirectToAction("Index", "Authenticate", values);
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