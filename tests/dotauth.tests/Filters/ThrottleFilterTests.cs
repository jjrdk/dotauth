namespace DotAuth.Tests.Filters;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotAuth.Telemetry;
using DotAuth.Tests.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

public sealed class ThrottleFilterTests
{
    [Fact]
    public async Task WhenThrottleRejectsRequestThenTelemetryIsRecorded()
    {
        var requestThrottle = Substitute.For<IRequestThrottle>();
        requestThrottle.Allow(Arg.Any<HttpRequest>()).Returns(false);
        var serviceProvider = new ServiceCollection()
            .AddSingleton(requestThrottle)
            .BuildServiceProvider();
        var filter = (IAsyncResourceFilter)new DotAuth.Filters.ThrottleFilter().CreateInstance(serviceProvider);
        var httpContext = new DefaultHttpContext();
        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor
            {
                AttributeRouteInfo = new AttributeRouteInfo { Template = "connect/token" }
            });
        var context = new ResourceExecutingContext(actionContext, new List<IFilterMetadata>(), new List<IValueProviderFactory>());
        using var activityCollector = new ActivityCollector();
        using var metricCollector = new MetricCollector(DotAuthTelemetry.MetricNames.ThrottleRejected);

        await filter.OnResourceExecutionAsync(context, () => throw new Xunit.Sdk.XunitException("next should not run"));

        var result = Assert.IsType<Microsoft.AspNetCore.Mvc.StatusCodeResult>(context.Result);
        Assert.Equal((int)HttpStatusCode.TooManyRequests, result.StatusCode);
        Assert.Contains(activityCollector.Activities, activity => activity.DisplayName == DotAuthTelemetry.ActivityNames.ThrottleCheck);
        Assert.Contains(metricCollector.Measurements, measurement => measurement.Name == DotAuthTelemetry.MetricNames.ThrottleRejected && measurement.Value == 1);
    }
}


