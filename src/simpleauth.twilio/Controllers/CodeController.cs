namespace SimpleAuth.Twilio.Controllers
{
    using Actions;
    using Microsoft.AspNetCore.Mvc;
    using Shared;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Errors;

    [Route(SmsConstants.CodeController)]
    public class CodeController : Controller
    {
        private readonly SmsAuthenticationOperation _smsAuthenticationOperation;

        public CodeController(
            ITwilioClient twilioClient,
            IConfirmationCodeStore confirmationCodeStore,
            IResourceOwnerRepository resourceOwnerRepository,
            ISubjectBuilder subjectBuilder,
            IEnumerable<IAccountFilter> accountFilters,
            IEventPublisher eventPublisher,
            SmsAuthenticationOptions smsOptions)
        {
            _smsAuthenticationOperation = new SmsAuthenticationOperation(
                twilioClient,
                confirmationCodeStore,
                resourceOwnerRepository,
                subjectBuilder,
                accountFilters,
                eventPublisher,
                smsOptions);
        }

        /// <summary>
        /// Send the confirmation code to the phone number.
        /// </summary>
        /// <param name="confirmationCodeRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Send(
            [FromBody] ConfirmationCodeRequest confirmationCodeRequest,
            CancellationToken cancellationToken)
        {
            var checkResult = Check(confirmationCodeRequest);
            if (checkResult != null)
            {
                return checkResult;
            }

            IActionResult result;
            try
            {
                await _smsAuthenticationOperation.Execute(confirmationCodeRequest.PhoneNumber, cancellationToken)
                    .ConfigureAwait(false);
                result = new OkResult();
            }
            catch (SimpleAuthException ex)
            {
                result = BuildError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception)
            {
                result = BuildError(
                    ErrorCodes.UnhandledExceptionCode,
                    "unhandled exception occured please contact the administrator",
                    HttpStatusCode.InternalServerError);
            }

            return result;
        }

        /// <summary>
        /// Check the parameter.
        /// </summary>
        /// <param name="confirmationCodeRequest"></param>
        /// <returns></returns>
        private IActionResult Check(ConfirmationCodeRequest confirmationCodeRequest)
        {
            if (confirmationCodeRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no request", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(confirmationCodeRequest.PhoneNumber))
            {
                return BuildError(
                    ErrorCodes.InvalidRequestCode,
                    "parameter phone_number is missing",
                    HttpStatusCode.BadRequest);
            }

            return null;
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
            var error = new ErrorResponse {Error = code, ErrorDescription = message};
            return new JsonResult(error) {StatusCode = (int) statusCode};
        }
    }
}