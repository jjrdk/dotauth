using Microsoft.AspNetCore.Builder;
using SimpleIdentityServer.Manager.Host.Middleware;
using SimpleIdentityServer.Manager.Logging;
using System;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseManagerApi(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseSimpleIdentityServerManagerExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                ManagerEventSource = (IManagerEventSource)app.ApplicationServices.GetService(typeof(IManagerEventSource))
            });
            return app;
        }
    }
}