namespace SimpleAuth.AcceptanceTests
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        [HttpGet]
        [Authorize("uma_ticket")]
        public string Index()
        {
            return "Hello";
        }
    }
}