namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using DotAuth.AcceptanceTests.Support;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Reqnroll;
using Xunit;

public partial class FeatureTest
{
    private OpenTelemetryCollectorContainer? _openTelemetryCollector;
    private Option<GrantedTokenResponse>? _refreshedToken;
    private Option<JwtPayload>? _userInfoResponse;
    private Option<UmaIntrospectionResponse>? _introspectionResponse;

    /// <summary>
    /// Starts a collector container and exposes OTLP settings through environment variables for the future server implementation.
    /// </summary>
    [Given(@"a running OpenTelemetry collector container")]
    public async Task GivenARunningOpenTelemetryCollectorContainer()
    {
        _openTelemetryCollector ??= new OpenTelemetryCollectorContainer();
        await _openTelemetryCollector.StartAsync().ConfigureAwait(false);
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", _openTelemetryCollector.OtlpHttpEndpoint.ToString());
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf");
        Environment.SetEnvironmentVariable("OTEL_SERVICE_NAME", "dotauth.acceptancetests");
    }

    /// <summary>
    /// Refreshes the current token so the scenario can assert refresh-token telemetry.
    /// </summary>
    [When(@"refreshing the token for telemetry verification")]
    public async Task WhenRefreshingTheTokenForTelemetryVerification()
    {
        _refreshedToken = await _tokenClient.GetToken(TokenRequest.FromRefreshToken(_token.RefreshToken!)).ConfigureAwait(false);
        Assert.IsType<Option<GrantedTokenResponse>.Result>(_refreshedToken);
    }

    /// <summary>
    /// Revokes the current token so the scenario can assert revocation telemetry.
    /// </summary>
    [When(@"revoking the token for telemetry verification")]
    public async Task WhenRevokingTheTokenForTelemetryVerification()
    {
        var revokeResponse = await _tokenClient.RevokeToken(RevokeTokenRequest.Create(_token)).ConfigureAwait(false);
        Assert.IsType<Option.Success>(revokeResponse);
    }

    /// <summary>
    /// Calls the introspection endpoint so the scenario can assert introspection telemetry.
    /// </summary>
    [When(@"introspecting the RPT token for telemetry verification")]
    public async Task WhenIntrospectingTheRptTokenForTelemetryVerification()
    {
        _introspectionResponse = await _umaClient
            .Introspect(DotAuth.Client.IntrospectionRequest.Create(_rptToken!, "access_token", _token.AccessToken))
            .ConfigureAwait(false);

        Assert.IsType<Option<UmaIntrospectionResponse>.Result>(_introspectionResponse);
    }

    /// <summary>
    /// Calls the user info endpoint so the scenario can assert user info telemetry.
    /// </summary>
    [When(@"getting user information for telemetry verification")]
    public async Task WhenGettingUserInformationForTelemetryVerification()
    {
        _userInfoResponse = await _tokenClient.GetUserInfo(_token.AccessToken).ConfigureAwait(false);
        Assert.IsType<Option<JwtPayload>.Result>(_userInfoResponse);
    }

    /// <summary>
    /// Verifies that the collector eventually exports every expected trace name.
    /// </summary>
    /// <param name="table">The expected trace names.</param>
    [Then(@"the collector eventually contains the following traces")]
    public async Task ThenTheCollectorEventuallyContainsTheFollowingTraces(Table table)
    {
        var expectedSpans = table.Rows.Select(row => row["span_name"]).ToArray();
        var exportedTraces = await EventuallyReadAsync(() => _openTelemetryCollector!.ReadTracesAsync(), expectedSpans)
            .ConfigureAwait(false);

        foreach (var expectedSpan in expectedSpans)
        {
            Assert.Contains(expectedSpan, exportedTraces, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Verifies that the collector eventually exports every expected metric name.
    /// </summary>
    /// <param name="table">The expected metric names.</param>
    [Then(@"the collector eventually contains the following metrics")]
    public async Task ThenTheCollectorEventuallyContainsTheFollowingMetrics(Table table)
    {
        var expectedMetrics = table.Rows.Select(row => row["metric_name"]).ToArray();
        var exportedMetrics = await EventuallyReadAsync(() => _openTelemetryCollector!.ReadMetricsAsync(), expectedMetrics)
            .ConfigureAwait(false);

        foreach (var expectedMetric in expectedMetrics)
        {
            Assert.Contains(expectedMetric, exportedMetrics, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Polls the collector until all expected values are exported or the timeout is reached.
    /// </summary>
    /// <param name="readAsync">The collector read operation.</param>
    /// <param name="expectedValues">The values that must all appear in the export.</param>
    /// <returns>The exported payload.</returns>
    private static async Task<string> EventuallyReadAsync(Func<Task<string>> readAsync, IReadOnlyCollection<string> expectedValues)
    {
        var timeout = TimeSpan.FromSeconds(15);
        var retryDelay = TimeSpan.FromMilliseconds(250);
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        var lastPayload = string.Empty;

        while (DateTimeOffset.UtcNow < deadline)
        {
            lastPayload = await readAsync().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(lastPayload)
                && expectedValues.All(expectedValue => lastPayload.Contains(expectedValue, StringComparison.Ordinal)))
            {
                return lastPayload;
            }

            await Task.Delay(retryDelay).ConfigureAwait(false);
        }

        return lastPayload;
    }
}


