namespace SimpleAuth.Sms.Controllers;

using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Errors;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Repositories;
using SimpleAuth.Sms.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleAuth.Events;
using SimpleAuth.Filters;
using SimpleAuth.Shared.Requests;

/// <summary>
/// Defines the code controller.
/// </summary>
/// <seealso cref="Controller" />
[Route(SmsConstants.CodeController)]
[ThrottleFilter]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class CodeController : ControllerBase
{
    private readonly SmsAuthenticationOperation _smsAuthenticationOperation;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeController"/> class.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="smsClient">The SMS client.</param>
    /// <param name="confirmationCodeStore">The confirmation code store.</param>
    /// <param name="resourceOwnerRepository">The resource owner repository.</param>
    /// <param name="subjectBuilder">The subject builder.</param>
    /// <param name="accountFilters">The account filters.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    /// <param name="logger">The logger</param>
    public CodeController(
        RuntimeSettings settings,
        ISmsClient smsClient,
        IConfirmationCodeStore confirmationCodeStore,
        IResourceOwnerRepository resourceOwnerRepository,
        ISubjectBuilder subjectBuilder,
        IEnumerable<IAccountFilter> accountFilters,
        IEventPublisher eventPublisher,
        ILogger<CodeController> logger)
    {
        _smsAuthenticationOperation = new SmsAuthenticationOperation(
            settings,
            smsClient,
            confirmationCodeStore,
            resourceOwnerRepository,
            subjectBuilder,
            accountFilters.ToArray(),
            eventPublisher,
            logger);
    }

    /// <summary>
    /// Send the confirmation code to the phone number.
    /// </summary>
    /// <param name="confirmationCodeRequest"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    [HttpPost]
    [ThrottleFilter]
    public async Task<IActionResult> Send(
        [FromBody] ConfirmationCodeRequest confirmationCodeRequest,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(confirmationCodeRequest.PhoneNumber))
        {
            return BuildError(
                ErrorCodes.InvalidRequest,
                "parameter phone_number is missing",
                HttpStatusCode.BadRequest);
        }

        try
        {
            var option = await _smsAuthenticationOperation.Execute(confirmationCodeRequest.PhoneNumber, cancellationToken)
                .ConfigureAwait(false);
            if (option is Option<ResourceOwner>.Error e)
            {
                return new ObjectResult(e.Details) { StatusCode = (int)e.Details.Status };
            }
            return new OkResult();
        }
        catch (Exception)
        {
            return BuildError(
                ErrorCodes.UnhandledExceptionCode,
                "unhandled exception occurred please contact the administrator",
                HttpStatusCode.InternalServerError);
        }
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