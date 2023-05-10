namespace DotAuth.AcceptanceTests.Support;

using DotAuth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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