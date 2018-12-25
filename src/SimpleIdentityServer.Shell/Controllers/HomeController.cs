namespace SimpleIdentityServer.Shell.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Server.Controllers.Website;
    using System.Threading.Tasks;

    [Area("Shell")]
    public class HomeController : BaseController
    {
        public HomeController(IAuthenticationService authenticationService) : base(authenticationService)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUser().ConfigureAwait(false);
            return View();
        }
    }
}
