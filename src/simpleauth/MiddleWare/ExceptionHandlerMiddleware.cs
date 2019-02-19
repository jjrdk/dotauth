// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.MiddleWare
{
    using Exceptions;
    using Microsoft.AspNetCore.Http;
    using Shared;
    using Shared.Responses;
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Events.Logging;

    internal class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEventPublisher _publisher;

        public ExceptionHandlerMiddleware(
            RequestDelegate next,
            IEventPublisher publisher)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _publisher = publisher;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                context.Response.Clear();

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                if (exception is SimpleAuthException serverException)
                {
                    var exceptionWithState = exception as SimpleAuthExceptionWithState;
                    var state = exceptionWithState == null
                        ? string.Empty
                        : exceptionWithState.State;
                    await _publisher.Publish(new SimpleAuthError(Id.Create(),
                        serverException.Code,
                        serverException.Message,
                        state,
                        DateTime.UtcNow)).ConfigureAwait(false);

                    if (exceptionWithState != null)
                    {
                        ErrorResponse errorResponseWithState = new ErrorResponseWithState
                        {
                            ErrorDescription = serverException.Message,
                            Error = serverException.Code,
                            State = exceptionWithState.State
                        };

                        var serializedError = errorResponseWithState.SerializeWithJavascript();
                        await context.Response.WriteAsync(serializedError).ConfigureAwait(false);
                    }
                    else
                    {
                        var error = new ErrorResponse
                        {
                            ErrorDescription = serverException.Message,
                            Error = serverException.Code
                        };

                        var serializedError = error.SerializeWithJavascript();
                        await context.Response.WriteAsync(serializedError).ConfigureAwait(false);
                    }
                }
                else
                {
                    //serverException = new SimpleAuthException(ErrorCodes.UnhandledExceptionCode, exception.Message);
                    await _publisher.Publish(new ExceptionMessage(
                        Id.Create(),
                        exception,
                        DateTime.UtcNow)).ConfigureAwait(false);
                    var error = new ErrorResponse
                    {
                        Error = ErrorCodes.UnhandledExceptionCode,
                        ErrorDescription = exception.Message
                    };

                    var serializedError = error.SerializeWithJavascript();
                    await context.Response.WriteAsync(serializedError).ConfigureAwait(false);
                }
            }
        }
    }
}
