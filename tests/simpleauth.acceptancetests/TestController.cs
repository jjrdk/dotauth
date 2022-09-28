namespace SimpleAuth.AcceptanceTests;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Shared;

[Route("[controller]")]
public sealed class TestController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public string Index()
    {
        var user = User;
        return "Hello " + user.GetName();
    }
}