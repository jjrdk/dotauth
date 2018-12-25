namespace SimpleAuth.Server.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;

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
