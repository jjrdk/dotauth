namespace SimpleAuth.Controllers
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Api.Introspection;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the introspection controller.
    /// </summary>
    /// <seealso cref="Controller" />
    [Route(UmaConstants.RouteValues.Introspection)]
    public class UmaIntrospectionController : ControllerBase
    {
        private readonly UmaIntrospectionAction _introspectionActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectionController"/> class.
        /// </summary>
        /// <param name="tokenStore">The token store.</param>
        public UmaIntrospectionController(ITokenStore tokenStore)
        {
            _introspectionActions = new UmaIntrospectionAction(tokenStore);
        }

        /// <summary>
        /// Handles the specified introspection request.
        /// </summary>
        /// <param name="introspectionRequest">The introspection request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(
            [FromForm] IntrospectionRequest introspectionRequest,
            CancellationToken cancellationToken)
        {
            if (introspectionRequest?.token == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var result = await _introspectionActions.Execute(
                    introspectionRequest.ToParameter(),
                    cancellationToken)
                .ConfigureAwait(false);
            return result.StatusCode == HttpStatusCode.OK
                ? Ok(result.Content)
                : (IActionResult)BadRequest(result.Error);
        }

        /// <summary>
        /// Build the JSON error message.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
            return new JsonResult(error) { StatusCode = (int)statusCode };
        }
    }
}