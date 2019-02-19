namespace SimpleAuth.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the home controller.
    /// </summary>
    /// <seealso cref="SimpleAuth.Controllers.BaseController" />
    public class HomeController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        public HomeController(IAuthenticationService authenticationService) : base(authenticationService)
        {
        }

        /// <summary>
        /// Handle the default GET request.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUser().ConfigureAwait(false);
            return RedirectToActionPermanent("Index", "Authenticate"); //View("Index");
        }
    }
}
