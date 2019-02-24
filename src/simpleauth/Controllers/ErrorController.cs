namespace SimpleAuth.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ViewModels;

    /// <summary>
    /// Defines the error controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class ErrorController : Controller
    {
        /// <summary>
        /// Get the default error page.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public ActionResult Index(string code, string message)
        {
            var viewModel = new ErrorViewModel
            {
                Code = code,
                Message = message
            };
            return View("Index", viewModel);
        }

        /// <summary>
        /// Gets the 401 error page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("401")]
        public ActionResult Get401()
        {
            return View("Get401");
        }

        /// <summary>
        /// Gets the 404 error page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("404")]
        public ActionResult Get404()
        {
            return View("Get404");
        }

        /// <summary>
        /// Gets the 500 error page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("500")]
        public ActionResult Get500()
        {
            return View("Get500");
        }
    }
}