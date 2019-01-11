namespace SimpleAuth.Server.Controllers
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
            return View(viewModel);
        }

        [HttpGet]
        [Route("401")]
        public ActionResult Get401()
        {
            return View();
        }

        [HttpGet]
        [Route("404")]
        public ActionResult Get404() 
        {
            return View();    
        }

        [HttpGet]
        [Route("500")]
        public ActionResult Get500()
        {
            return View();
        }
    }
}