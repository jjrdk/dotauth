namespace SimpleAuth.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

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
            return new ThrottleFilterAttribute((IRequestThrottle)serviceProvider.GetService(typeof(IRequestThrottle)));
        }

        private class ThrottleFilterAttribute : IAsyncResourceFilter
        {
            private readonly IRequestThrottle _requestThrottle;

            /// <inheritdoc />
            public ThrottleFilterAttribute(IRequestThrottle requestThrottle)
            {
                _requestThrottle = requestThrottle;
            }

            /// <inheritdoc />
            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                if (!await _requestThrottle.Allow(context.HttpContext.Request).ConfigureAwait(false))
                {
                    context.Result = new StatusCodeResult(429);
                    return;
                }

                await next().ConfigureAwait(false);
            }
        }
    }
}