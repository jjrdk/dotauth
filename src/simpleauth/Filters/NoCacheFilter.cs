namespace SimpleAuth.Filters
{
    using System;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Defines the throttling filter attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    internal class NoCacheFilter : Attribute, IFilterFactory
    {
        /// <inheritdoc />
        public bool IsReusable => true;

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new NoCacheFilterAttribute();
        }

        private class NoCacheFilterAttribute : IResultFilter
        {
            /// <inheritdoc />
            public void OnResultExecuted(ResultExecutedContext context)
            {
            }

            /// <inheritdoc />
            public void OnResultExecuting(ResultExecutingContext context)
            {
                context.HttpContext.Response.Headers[HeaderNames.CacheControl] = "no-cache";
            }
        }
    }
}