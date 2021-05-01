namespace SimpleAuth.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Filters;

    /// <summary>
    /// Defines the home controller.
    /// </summary>
    /// <seealso cref="BaseController" />
    [ThrottleFilter]
    public class HomeController : BaseController
    {
        private readonly RuntimeSettings _settings;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        public HomeController(
            IAuthenticationService authenticationService,
            RuntimeSettings settings,
            ILogger<HomeController> logger)
            : base(authenticationService)
        {
            _settings = settings;
            _logger = logger;
        }

        /// <summary>
        /// Handle the default GET request.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUser().ConfigureAwait(false);
            if (_settings.RedirectToLogin)
            {
                _logger.LogDebug("Redirecting to login page");
                return RedirectToAction("Index", "Authenticate");
            }

            return Ok(new object());
        }
    }
}
