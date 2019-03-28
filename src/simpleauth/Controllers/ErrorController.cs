namespace SimpleAuth.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Shared.Models;
    using System;
    using System.Net;
    using ViewModels;

    /// <summary>
    /// Defines the error controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("error")]
    public class ErrorController : Controller
    {
        /// <summary>
        /// Get the default error page.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="title"></param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public ActionResult Index(string code, string title, string message)
        {
            if (!Enum.TryParse<HttpStatusCode>(code, out var statusCode))
            {
                statusCode = HttpStatusCode.BadRequest;
            }

            if (Request.ContentType == "text/html")
            {
                var viewModel = new ErrorViewModel { Code = (int)statusCode, Title = title, Message = message };
                return View("Index", viewModel);
            }

            return StatusCode(
                (int) statusCode,
                new ErrorDetails {Detail = message, Status = statusCode, Title = title});
        }

        /// <summary>
        /// Gets the 400 error page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("400")]
        public ActionResult Get400()
        {
            return Index(
                "400",
                "Bad Request",
                "The HyperText Transfer Protocol (HTTP) 400 Bad Request response status code indicates that the server cannot or will not process the request due to something that is perceived to be a client error (e.g., malformed request syntax, invalid request message framing, or deceptive request routing).");
        }

        /// <summary>
        /// Gets the 401 error page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("401")]
        public ActionResult Get401()
        {
            return Index(
                "401",
                "Unauthorized",
                "The HTTP 401 Unauthorized client error status response code indicates that the request has not been applied because it lacks valid authentication credentials for the target resource.");
        }

        /// <summary>
        /// Gets the 404 error page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("404")]
        public ActionResult Get404()
        {
            return Index(
                "404",
                "Not Found",
                "The HTTP 404 Not Found client error response code indicates that the server can't find the requested resource.");
        }

        /// <summary>
        /// Gets the 500 error page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("500")]
        public ActionResult Get500()
        {
            return Index(
                "500",
                "Internal Server Error",
                "The HyperText Transfer Protocol (HTTP) 500 Internal Server Error server error response code indicates that the server encountered an unexpected condition that prevented it from fulfilling the request.");
        }
    }
}
