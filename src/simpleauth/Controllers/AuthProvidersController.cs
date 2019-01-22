namespace SimpleAuth.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;

    [Route("authproviders")]
    public class AuthProvidersController : Controller
    {
        protected readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public AuthProvidersController(IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var schemes = (await _authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).Where(p => !string.IsNullOrWhiteSpace(p.DisplayName));
            var result = schemes.ToDtos();
            return new OkObjectResult(result);
        }
    }
}
