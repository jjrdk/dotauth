namespace SimpleAuth.Sms.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Sms.Actions;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Controllers;

    /// <summary>
    /// Defines the code controller.
    /// </summary>
    /// <seealso cref="Controller" />
    [Route(SmsConstants.CodeController)]
    public class CodeController : ControllerBase
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
        public CodeController(
            RuntimeSettings settings,
            ISmsClient smsClient,
            IConfirmationCodeStore confirmationCodeStore,
            IResourceOwnerRepository resourceOwnerRepository,
            ISubjectBuilder subjectBuilder,
            IEnumerable<IAccountFilter> accountFilters,
            IEventPublisher eventPublisher)
        {
            _smsAuthenticationOperation = new SmsAuthenticationOperation(
                settings,
                smsClient,
                confirmationCodeStore,
                resourceOwnerRepository,
                subjectBuilder,
                accountFilters,
                eventPublisher);
        }

        /// <summary>
        /// Send the confirmation code to the phone number.
        /// </summary>
        /// <param name="confirmationCodeRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ThrottleFilter]
        public async Task<IActionResult> Send(
            [FromBody] ConfirmationCodeRequest confirmationCodeRequest,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(confirmationCodeRequest?.PhoneNumber))
            {
                return BuildError(
                    ErrorCodes.InvalidRequestCode,
                    "parameter phone_number is missing",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                await _smsAuthenticationOperation.Execute(confirmationCodeRequest.PhoneNumber, cancellationToken)
                    .ConfigureAwait(false);
                return new OkResult();
            }
            catch (SimpleAuthException ex)
            {
                return BuildError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception)
            {
                return BuildError(
                    ErrorCodes.UnhandledExceptionCode,
                    "unhandled exception occured please contact the administrator",
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
}
