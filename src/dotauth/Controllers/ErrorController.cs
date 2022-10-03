namespace DotAuth.Controllers;

using System;
using System.Net;
using DotAuth.Properties;
using DotAuth.Shared.Models;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Defines the error controller
/// </summary>
/// <seealso cref="Controller" />
[Route("error")]
public sealed class ErrorController : Controller
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

        return StatusCode(
            (int)statusCode,
            new ErrorDetails { Detail = message, Status = statusCode, Title = title });
    }

    /// <summary>
    /// Gets the 400 error page.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("400")]
    [ResponseCache(Duration = 86400)]
    public ActionResult Get400()
    {
        return Index(
            "400",
            Strings.Badrequest,
            Strings.Http400);
    }

    /// <summary>
    /// Gets the 401 error page.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("401")]
    [ResponseCache(Duration = 86400)]
    public ActionResult Get401()
    {
        return Index(
            "401",
            Strings.Unauthorized,
            Strings.Http401);
    }

    /// <summary>
    /// Gets the 404 error page.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("404")]
    [ResponseCache(Duration = 86400)]
    public ActionResult Get404()
    {
        return Index(
            "404",
            Strings.NotFound,
            Strings.Http404);
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
            Strings.InternalServerError,
            Strings.Http500);
    }
}