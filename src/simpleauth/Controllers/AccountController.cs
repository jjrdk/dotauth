namespace SimpleAuth.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.ViewModels;

    [Route("Account")]
    public class AccountController : BaseController
    {
        public AccountController(IAuthenticationService authenticationService)
            : base(authenticationService)
        {
        }

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

        [HttpGet("AccessDenied")]
        public async Task<IActionResult> AccessDenied()
        {
            return Request.Query.TryGetValue("ReturnUrl", out var returnUrl)
                ? RedirectToAction("Index", "Authenticate", new {ReturnUrl = returnUrl})
                : RedirectToAction("Index", "Authenticate");
        }

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