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
    using SimpleAuth.Shared.Events.Logging;
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http.Features;
    using SimpleAuth.Events;
    using SimpleAuth.Shared.Errors;

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

                if (exception is SimpleAuthException serverException)
                {
                    var state = exception is SimpleAuthExceptionWithState exceptionWithState
                        ? exceptionWithState.State
                        : string.Empty;
                    await _publisher.Publish(new SimpleAuthError(Id.Create(),
                        serverException.Code,
                        serverException.Message,
                        state,
                        DateTimeOffset.UtcNow)).ConfigureAwait(false);

                    context.Request.Path = "/error";
                    context.Request.QueryString = new QueryString()
                        .Add("code", "400")
                        .Add("title", serverException.Code)
                        .Add("message", exception.Message);
                }
                else
                {
                    await _publisher.Publish(new SimpleAuthError(
                        Id.Create(),
                        exception.GetType().Name,
                        exception.Message,
                        string.Empty,
                        DateTimeOffset.UtcNow)).ConfigureAwait(false);

                    context.Request.Path = "/error";
                    context.Request.QueryString = new QueryString()
                        .Add("code", "500")
                        .Add("title", ErrorCodes.UnhandledExceptionCode)
                        .Add("message", exception.Message);
                }

                if (!(context.Features[typeof(IEndpointFeature)] is IEndpointFeature endpointFeature))
                {
                    context.Response.Clear();
                    context.Response.StatusCode = 500;
                    return;
                }

                endpointFeature.Endpoint = null;
                await Invoke(context).ConfigureAwait(false);
            }
        }
    }
}
