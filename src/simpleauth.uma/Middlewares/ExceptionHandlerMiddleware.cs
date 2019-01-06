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

namespace SimpleAuth.Uma.Middlewares
{
    using Exceptions;
    using Logging;
    using Microsoft.AspNetCore.Http;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using ErrorCodes = SimpleAuth.Errors.ErrorCodes;

    internal class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEventPublisher _eventPublisher;

        public ExceptionHandlerMiddleware(RequestDelegate next, IEventPublisher eventPublisher)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _eventPublisher = eventPublisher;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                if (!(exception is BaseUmaException serverException))
                {
                    serverException = new BaseUmaException(ErrorCodes.UnhandledExceptionCode, exception.Message);
                }

                await _eventPublisher.Publish(new ExceptionMessage(
                    Id.Create(),
                    exception,
                    DateTime.UtcNow)).ConfigureAwait(false);
                //_options.UmaEventSource.Failure(serverException);
                var error = new ErrorResponse
                {
                    Error = serverException.Code,
                    ErrorDescription = serverException.Message
                };
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                var serializedError = error.SerializeWithDataContract();
                await context.Response.WriteAsync(serializedError).ConfigureAwait(false);
            }
        }
    }
}
