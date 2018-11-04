using Microsoft.AspNetCore.Http;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Host.Middleware
{
    using Shared.Responses;

    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ExceptionHandlerMiddlewareOptions _options;

        public ExceptionHandlerMiddleware(RequestDelegate next, ExceptionHandlerMiddlewareOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var identityServerManagerException = exception as IdentityServerManagerException;
                var idServerException = exception as IdentityServerException;
                var code = ErrorCodes.UnhandledExceptionCode;
                if (identityServerManagerException != null)
                {
                    code = identityServerManagerException.Code;
                }

                if (idServerException != null)
                {
                    code = idServerException.Code;
                }

                var errorResponse = new ErrorResponse
                {
                    Error = code,
                    ErrorDescription = exception.Message
                };

                _options.ManagerEventSource.Failure(identityServerManagerException);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                var serializedErrorResponse = errorResponse.SerializeWithDataContract();
                await context.Response.WriteAsync(serializedErrorResponse).ConfigureAwait(false);
            }
        }
    }
}
