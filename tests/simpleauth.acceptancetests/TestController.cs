namespace SimpleAuth.AcceptanceTests
{
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using SimpleAuth.Extensions;
    using SimpleAuth.ResourceServer;

    [Route("[controller]")]
    public class TestController : ControllerBase
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