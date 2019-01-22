namespace SimpleAuth.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ViewModels;

    public class ErrorController : Controller
    {
        public ActionResult Index(string code, string message)
        {
            var viewModel = new ErrorViewModel
            {
                Code = code,
                Message = message
            };
            return View("Index", viewModel);
        }

        [HttpGet]
        [Route("401")]
        public ActionResult Get401()
        {
            return View("Get401");
        }

        [HttpGet]
        [Route("404")]
        public ActionResult Get404()
        {
            return View("Get404");
        }

        [HttpGet]
        [Route("500")]
        public ActionResult Get500()
        {
            return View("Get500");
        }
    }
}