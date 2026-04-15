namespace DotAuth.Filters;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using DotAuth.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Defines the throttling filter attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ThrottleFilter : Attribute, IFilterFactory
{
    /// <inheritdoc />
    public bool IsReusable
    {
        get { return true; }
    }

    /// <inheritdoc />
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new ThrottleFilterAttribute(serviceProvider.GetRequiredService<IRequestThrottle>());
    }

    private sealed class ThrottleFilterAttribute : IAsyncResourceFilter
    {
        private readonly IRequestThrottle _requestThrottle;

        public ThrottleFilterAttribute(IRequestThrottle requestThrottle)
        {
            _requestThrottle = requestThrottle;
        }

        /// <inheritdoc />
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var route = context.ActionDescriptor.AttributeRouteInfo?.Template ?? context.HttpContext.Request.Path.Value;
            using var activity = DotAuthTelemetry.StartInternalActivity(DotAuthTelemetry.ActivityNames.ThrottleCheck);
            activity?.SetTag(DotAuthTelemetry.TagKeys.HttpRoute, DotAuthTelemetry.Normalize(route));
            var allowed = await _requestThrottle.Allow(context.HttpContext.Request).ConfigureAwait(false);
            activity?.SetTag(DotAuthTelemetry.TagKeys.ThrottleAllowed, allowed);
            DotAuthTelemetry.RecordThrottleCheck(route, allowed);
            if (!allowed)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "rejected");
                context.Result = new StatusCodeResult((int)HttpStatusCode.TooManyRequests);
                return;
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            await next().ConfigureAwait(false);
        }
    }
}
