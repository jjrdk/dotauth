namespace SimpleAuth.Uma.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Responses;
    
    [Route(UmaConstants.RouteValues.Registration)]
    public class RegistrationController : Controller
    {
        private readonly IClientRepository _repository;

        public RegistrationController(IClientRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Client client)
        {
            if (client == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var result = await _repository.Insert(client).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
