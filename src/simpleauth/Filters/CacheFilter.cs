namespace SimpleAuth.Filters
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Defines the throttling filter attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    internal class CacheFilter : Attribute, IFilterFactory
    {
        /// <inheritdoc />
        public bool IsReusable => true;

        public int Duration { get; set; } = 86400;

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new CacheFilterAttribute(Duration);
        }

        private class CacheFilterAttribute : IResultFilter
        {
            private readonly int _duration;

            public CacheFilterAttribute(int duration)
            {
                _duration = duration;
            }

            /// <inheritdoc />
            public void OnResultExecuted(ResultExecutedContext context)
            {
            }

            /// <inheritdoc />
            public void OnResultExecuting(ResultExecutingContext context)
            {
                context.HttpContext.Response.Headers[HeaderNames.CacheControl] = "public, max-age=" + _duration;
            }
        }
    }
}