namespace DotAuth.Tests.Telemetry;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using DotAuth.Telemetry;

/// <summary>
/// Captures DotAuth activities emitted during a test.
/// </summary>
internal sealed class ActivityCollector : IDisposable
{
    private readonly ActivityListener _listener;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityCollector"/> class.
    /// </summary>
    public ActivityCollector()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == DotAuthTelemetry.ActivitySourceName,
            Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => Activities.Add(activity)
        };
        ActivitySource.AddActivityListener(_listener);
    }

    /// <summary>
    /// Gets the captured activities.
    /// </summary>
    public List<Activity> Activities { get; } = [];

    /// <inheritdoc />
    public void Dispose()
    {
        _listener.Dispose();
    }
}

/// <summary>
/// Captures selected DotAuth metric measurements emitted during a test.
/// </summary>
internal sealed class MetricCollector : IDisposable
{
    private readonly MeterListener _listener = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricCollector"/> class.
    /// </summary>
    /// <param name="instrumentNames">The instrument names to capture.</param>
    public MetricCollector(params string[] instrumentNames)
    {
        var selectedInstrumentNames = new HashSet<string>(instrumentNames, StringComparer.Ordinal);
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == DotAuthTelemetry.MeterName && selectedInstrumentNames.Contains(instrument.Name))
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        _listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
        {
            Measurements.Add((instrument.Name, measurement));
        });
        _listener.RecordObservableInstruments();
        _listener.Start();
    }

    /// <summary>
    /// Gets the captured long-counter measurements.
    /// </summary>
    public List<(string Name, long Value)> Measurements { get; } = [];

    /// <inheritdoc />
    public void Dispose()
    {
        _listener.Dispose();
    }
}


