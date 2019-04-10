namespace SimpleAuth.AcceptanceTests
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Extensions;

    [Route("[controller]")]
    public class TestController : Controller
    {
        [Authorize]
        [HttpGet]
        public string Index()
        {
            var user = User;
            return "Hello " + user.GetName();
        }
    }
}