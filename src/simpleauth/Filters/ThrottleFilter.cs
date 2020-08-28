namespace SimpleAuth.Filters
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Defines the throttling filter attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ThrottleFilter : Attribute, IFilterFactory
    {
        /// <inheritdoc />
        public bool IsReusable => true;

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new ThrottleFilterAttribute(serviceProvider.GetRequiredService<IRequestThrottle>());
        }

        private class ThrottleFilterAttribute : IAsyncResourceFilter
        {
            private readonly IRequestThrottle _requestThrottle;

            public ThrottleFilterAttribute(IRequestThrottle requestThrottle)
            {
                _requestThrottle = requestThrottle;
            }

            /// <inheritdoc />
            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                if (!await _requestThrottle.Allow(context.HttpContext.Request).ConfigureAwait(false))
                {
                    context.Result = new StatusCodeResult((int)HttpStatusCode.TooManyRequests);
                    return;
                }

                await next().ConfigureAwait(false);
            }
        }
    }
}