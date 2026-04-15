namespace DotAuth.Tests.MiddleWare;

using System;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.MiddleWare;
using DotAuth.Shared.Events.Logging;
using DotAuth.Telemetry;
using DotAuth.Tests.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

public sealed class ExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task WhenRequestThrowsThenTelemetryAndErrorEventAreRecorded()
    {
        var publisher = Substitute.For<IEventPublisher>();
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw new InvalidOperationException("boom"),
            publisher,
            NullLogger<ExceptionHandlerMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Request.Path = "/connect/token";
        using var activityCollector = new ActivityCollector();
        using var metricCollector = new MetricCollector(DotAuthTelemetry.MetricNames.UnhandledErrors);

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains(activityCollector.Activities, activity => activity.DisplayName == DotAuthTelemetry.ActivityNames.Exception);
        Assert.Contains(metricCollector.Measurements, measurement => measurement.Name == DotAuthTelemetry.MetricNames.UnhandledErrors && measurement.Value == 1);
        await publisher.Received(1).Publish(Arg.Is<DotAuthError>(error =>
            error.Code == nameof(InvalidOperationException) && error.Description == "boom"));
    }
}



