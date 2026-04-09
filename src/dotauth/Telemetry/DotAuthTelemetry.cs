namespace DotAuth.Telemetry;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using DotAuth.Shared.Models;

/// <summary>
/// Centralizes custom OpenTelemetry activities and metrics for the DotAuth token server.
/// </summary>
internal static class DotAuthTelemetry
{
    /// <summary>
    /// The activity source name used by DotAuth custom traces.
    /// </summary>
    public const string ActivitySourceName = "DotAuth.TokenServer";

    /// <summary>
    /// The meter name used by DotAuth custom metrics.
    /// </summary>
    public const string MeterName = "DotAuth.TokenServer.Metrics";

    private static readonly ActivitySource ActivitySourceInstance = new(ActivitySourceName);
    private static readonly Meter MeterInstance = new(MeterName);
    private static readonly Counter<long> TokensIssuedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.tokens.issued",
        unit: "{tokens}",
        description: "Counts successful token responses.");
    private static readonly Counter<long> TokensReusedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.tokens.reused",
        unit: "{tokens}",
        description: "Counts token responses that reused an existing token.");
    private static readonly Histogram<double> TokenIssuanceDurationHistogram = MeterInstance.CreateHistogram<double>(
        "dotauth.token.issuance.duration",
        unit: "ms",
        description: "Measures token endpoint duration.");
    private static readonly Counter<long> TokenIssueFailuresCounter = MeterInstance.CreateCounter<long>(
        "dotauth.tokens.issue.failures",
        unit: "{requests}",
        description: "Counts failed token responses.");
    private static readonly Counter<long> TokensRevokedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.tokens.revoked",
        unit: "{tokens}",
        description: "Counts successful token revocations.");
    private static readonly Counter<long> TokenRevokeFailuresCounter = MeterInstance.CreateCounter<long>(
        "dotauth.tokens.revoke.failures",
        unit: "{requests}",
        description: "Counts failed token revocation attempts.");
    private static readonly Counter<long> RefreshTokensUsedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.refresh_tokens.used",
        unit: "{tokens}",
        description: "Counts successful refresh-token grants.");
    private static readonly Counter<long> RefreshTokensInvalidCounter = MeterInstance.CreateCounter<long>(
        "dotauth.refresh_tokens.invalid",
        unit: "{tokens}",
        description: "Counts invalid refresh-token grant attempts.");
    private static readonly Counter<long> DeviceAuthorizationStartedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.device_authorization.started",
        unit: "{requests}",
        description: "Counts started device authorization flows.");
    private static readonly Counter<long> DeviceCodePollsCounter = MeterInstance.CreateCounter<long>(
        "dotauth.device_code.polls",
        unit: "{polls}",
        description: "Counts device code polling attempts.");
    private static readonly Counter<long> AuthorizationCodesIssuedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.authorization_codes.issued",
        unit: "{codes}",
        description: "Counts authorization code flow requests that advance authorization code issuance.");
    private static readonly Counter<long> AuthorizationCodesRedeemedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.authorization_codes.redeemed",
        unit: "{codes}",
        description: "Counts authorization codes successfully exchanged for tokens.");
    private static readonly Counter<long> AuthorizationCodesExpiredCounter = MeterInstance.CreateCounter<long>(
        "dotauth.authorization_codes.expired",
        unit: "{codes}",
        description: "Counts authorization codes rejected because they expired.");
    private static readonly Counter<long> AuthorizationCodesInvalidCounter = MeterInstance.CreateCounter<long>(
        "dotauth.authorization_codes.invalid",
        unit: "{codes}",
        description: "Counts authorization codes rejected for non-expiry reasons.");
    private static readonly Counter<long> IntrospectionRequestsCounter = MeterInstance.CreateCounter<long>(
        "dotauth.introspection.requests",
        unit: "{requests}",
        description: "Counts token introspection requests.");
    private static readonly Counter<long> IntrospectionActiveCounter = MeterInstance.CreateCounter<long>(
        "dotauth.introspection.active",
        unit: "{tokens}",
        description: "Counts introspection responses that returned active tokens.");
    private static readonly Counter<long> IntrospectionInactiveCounter = MeterInstance.CreateCounter<long>(
        "dotauth.introspection.inactive",
        unit: "{tokens}",
        description: "Counts introspection responses that returned inactive or missing tokens.");
    private static readonly Counter<long> UserInfoRequestsCounter = MeterInstance.CreateCounter<long>(
        "dotauth.userinfo.requests",
        unit: "{requests}",
        description: "Counts user info requests.");
    private static readonly Counter<long> ClientAuthSuccessCounter = MeterInstance.CreateCounter<long>(
        "dotauth.client.auth.success",
        unit: "{attempts}",
        description: "Counts successful client authentication attempts.");
    private static readonly Counter<long> ClientAuthFailureCounter = MeterInstance.CreateCounter<long>(
        "dotauth.client.auth.failure",
        unit: "{attempts}",
        description: "Counts failed client authentication attempts.");
    private static readonly Counter<long> ResourceOwnerAuthSuccessCounter = MeterInstance.CreateCounter<long>(
        "dotauth.resource_owner.auth.success",
        unit: "{attempts}",
        description: "Counts successful resource-owner authentications.");
    private static readonly Counter<long> ResourceOwnerAuthFailureCounter = MeterInstance.CreateCounter<long>(
        "dotauth.resource_owner.auth.failure",
        unit: "{attempts}",
        description: "Counts failed resource-owner authentications.");
    private static readonly Counter<long> UmaRptIssuedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.uma.rpt.issued",
        unit: "{tokens}",
        description: "Counts issued UMA requesting-party tokens.");
    private static readonly Counter<long> UmaRptRequestSubmittedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.uma.rpt.request_submitted",
        unit: "{requests}",
        description: "Counts UMA requests that require interactive authorization.");
    private static readonly Counter<long> UmaRptDeniedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.uma.rpt.denied",
        unit: "{requests}",
        description: "Counts UMA requests denied by policy.");
    private static readonly Counter<long> UmaTicketExpiredCounter = MeterInstance.CreateCounter<long>(
        "dotauth.uma.ticket.expired",
        unit: "{tickets}",
        description: "Counts UMA ticket lookups that failed because the ticket expired.");
    private static readonly Counter<long> OAuthErrorsCounter = MeterInstance.CreateCounter<long>(
        "dotauth.errors.oauth",
        unit: "{errors}",
        description: "Counts OAuth protocol errors returned to callers.");
    private static readonly Counter<long> UnhandledErrorsCounter = MeterInstance.CreateCounter<long>(
        "dotauth.errors.unhandled",
        unit: "{errors}",
        description: "Counts unhandled exceptions captured by middleware.");
    private static readonly Counter<long> ThrottleAllowedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.throttle.allowed",
        unit: "{requests}",
        description: "Counts requests allowed by the throttle filter.");
    private static readonly Counter<long> ThrottleRejectedCounter = MeterInstance.CreateCounter<long>(
        "dotauth.throttle.rejected",
        unit: "{requests}",
        description: "Counts requests rejected by the throttle filter.");
    private static readonly Histogram<double> DeviceCodeApprovalDurationHistogram = MeterInstance.CreateHistogram<double>(
        "dotauth.device_code.approval.duration",
        unit: "s",
        description: "Measures time from device authorization issuance to approval.");
    private static readonly Histogram<double> TokenStoreOperationDurationHistogram = MeterInstance.CreateHistogram<double>(
        "dotauth.token_store.operation.duration",
        unit: "ms",
        description: "Measures token-store operation latency.");

    /// <summary>
    /// Starts a server activity for a DotAuth endpoint.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <returns>The started activity when listeners are configured.</returns>
    public static Activity? StartServerActivity(string name)
    {
        return ActivitySourceInstance.StartActivity(name, ActivityKind.Server);
    }

    /// <summary>
    /// Starts an internal activity for a DotAuth sub-operation.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <returns>The started activity when listeners are configured.</returns>
    public static Activity? StartInternalActivity(string name)
    {
        return ActivitySourceInstance.StartActivity(name, ActivityKind.Internal);
    }

    /// <summary>
    /// Records a successful token response.
    /// </summary>
    /// <param name="grantType">The OAuth grant type.</param>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordTokenIssued(string? grantType, string? clientId)
    {
        TokensIssuedCounter.Add(1, BuildCommonTags(grantType, clientId));
    }

    /// <summary>
    /// Records that a token response reused an existing token.
    /// </summary>
    /// <param name="grantType">The OAuth grant type.</param>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordTokenReused(string? grantType, string? clientId)
    {
        TokensReusedCounter.Add(1, BuildCommonTags(grantType, clientId));
    }

    /// <summary>
    /// Records a failed token response.
    /// </summary>
    /// <param name="grantType">The OAuth grant type.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="errorCode">The OAuth error code.</param>
    public static void RecordTokenIssueFailure(string? grantType, string? clientId, string? errorCode)
    {
        var tags = BuildCommonTags(grantType, clientId);
        tags.Add("dotauth.error_code", Normalize(errorCode));
        TokenIssueFailuresCounter.Add(1, tags);
        RecordOAuthError(errorCode);
    }

    /// <summary>
    /// Records token endpoint duration.
    /// </summary>
    /// <param name="durationMs">The request duration in milliseconds.</param>
    /// <param name="grantType">The OAuth grant type.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="success">Whether the response was successful.</param>
    public static void RecordTokenIssuanceDuration(double durationMs, string? grantType, string? clientId, bool success)
    {
        var tags = BuildCommonTags(grantType, clientId);
        tags.Add("success", success);
        TokenIssuanceDurationHistogram.Record(durationMs, tags);
    }

    /// <summary>
    /// Records a successful token revocation.
    /// </summary>
    /// <param name="tokenType">The revoked token type.</param>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordTokenRevoked(string? tokenType, string? clientId)
    {
        var tags = new TagList
        {
            { "dotauth.revoke.token_type", Normalize(tokenType) },
            { "dotauth.client_id", Normalize(clientId) }
        };
        TokensRevokedCounter.Add(1, tags);
    }

    /// <summary>
    /// Records a failed token revocation.
    /// </summary>
    /// <param name="tokenType">The requested token type.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="errorCode">The OAuth error code.</param>
    public static void RecordTokenRevokeFailure(string? tokenType, string? clientId, string? errorCode)
    {
        var tags = new TagList
        {
            { "dotauth.revoke.token_type", Normalize(tokenType) },
            { "dotauth.client_id", Normalize(clientId) },
            { "dotauth.error_code", Normalize(errorCode) }
        };
        TokenRevokeFailuresCounter.Add(1, tags);
        RecordOAuthError(errorCode);
    }

    /// <summary>
    /// Records a successful refresh token grant.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordRefreshTokenUsed(string? clientId)
    {
        RefreshTokensUsedCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records an invalid refresh token attempt.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordRefreshTokenInvalid(string? clientId)
    {
        RefreshTokensInvalidCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records the start of a device authorization flow.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordDeviceAuthorizationStarted(string? clientId)
    {
        DeviceAuthorizationStartedCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records a device code polling result.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="status">The device polling status.</param>
    public static void RecordDeviceCodePoll(string? clientId, string status)
    {
        DeviceCodePollsCounter.Add(
            1,
            new TagList
            {
                { "dotauth.client_id", Normalize(clientId) },
                { "status", Normalize(status) }
            });
    }

    /// <summary>
    /// Records authorization code flow activity for authorization requests.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordAuthorizationCodeIssued(string? clientId)
    {
        AuthorizationCodesIssuedCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records a successful authorization code redemption.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordAuthorizationCodeRedeemed(string? clientId)
    {
        AuthorizationCodesRedeemedCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records an expired authorization code.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordAuthorizationCodeExpired(string? clientId)
    {
        AuthorizationCodesExpiredCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records an invalid authorization code attempt.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="errorCode">The OAuth error code.</param>
    public static void RecordAuthorizationCodeInvalid(string? clientId, string? errorCode)
    {
        AuthorizationCodesInvalidCounter.Add(
            1,
            new TagList
            {
                { "dotauth.client_id", Normalize(clientId) },
                { "dotauth.error_code", Normalize(errorCode) }
            });
    }

    /// <summary>
    /// Records an introspection request.
    /// </summary>
    /// <param name="tokenFound">Whether the token was found.</param>
    /// <param name="tokenActive">Whether the token was active.</param>
    public static void RecordIntrospectionRequest(bool tokenFound, bool tokenActive)
    {
        var tags = new TagList
        {
            { "dotauth.introspection.token_found", tokenFound },
            { "dotauth.introspection.token_active", tokenActive }
        };
        IntrospectionRequestsCounter.Add(1, tags);
        if (tokenActive)
        {
            IntrospectionActiveCounter.Add(1);
            return;
        }

        IntrospectionInactiveCounter.Add(1);
    }

    /// <summary>
    /// Records a user info request.
    /// </summary>
    /// <param name="tokenValid">Whether the supplied access token was valid.</param>
    public static void RecordUserInfoRequest(bool tokenValid)
    {
        UserInfoRequestsCounter.Add(1, new TagList { { "dotauth.userinfo.token_valid", tokenValid } });
    }

    /// <summary>
    /// Records a successful client authentication.
    /// </summary>
    /// <param name="authMethod">The client authentication method.</param>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordClientAuthenticationSuccess(string? authMethod, string? clientId)
    {
        ClientAuthSuccessCounter.Add(1, BuildClientAuthTags(authMethod, clientId));
    }

    /// <summary>
    /// Records a failed client authentication.
    /// </summary>
    /// <param name="authMethod">The client authentication method.</param>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordClientAuthenticationFailure(string? authMethod, string? clientId)
    {
        ClientAuthFailureCounter.Add(1, BuildClientAuthTags(authMethod, clientId));
    }

    /// <summary>
    /// Records a successful resource-owner authentication.
    /// </summary>
    /// <param name="amr">The authentication method reference.</param>
    public static void RecordResourceOwnerAuthenticationSuccess(string? amr)
    {
        ResourceOwnerAuthSuccessCounter.Add(1, new TagList { { "dotauth.amr", Normalize(amr) } });
    }

    /// <summary>
    /// Records a failed resource-owner authentication.
    /// </summary>
    /// <param name="amr">The authentication method reference.</param>
    public static void RecordResourceOwnerAuthenticationFailure(string? amr)
    {
        ResourceOwnerAuthFailureCounter.Add(1, new TagList { { "dotauth.amr", Normalize(amr) } });
    }

    /// <summary>
    /// Records a successfully issued UMA requesting-party token.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordUmaRptIssued(string? clientId)
    {
        UmaRptIssuedCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records a UMA request that was submitted for later authorization.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordUmaRptRequestSubmitted(string? clientId)
    {
        UmaRptRequestSubmittedCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records a UMA request denied by policy.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordUmaRptDenied(string? clientId)
    {
        UmaRptDeniedCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records an expired UMA ticket.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordUmaTicketExpired(string? clientId)
    {
        UmaTicketExpiredCounter.Add(1, new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records an OAuth error response.
    /// </summary>
    /// <param name="errorCode">The OAuth error code.</param>
    public static void RecordOAuthError(string? errorCode)
    {
        OAuthErrorsCounter.Add(1, new TagList { { "dotauth.error_code", Normalize(errorCode) } });
    }

    /// <summary>
    /// Records an unhandled exception.
    /// </summary>
    /// <param name="exceptionType">The exception type.</param>
    /// <param name="route">The request route.</param>
    public static void RecordUnhandledException(string? exceptionType, string? route)
    {
        UnhandledErrorsCounter.Add(
            1,
            new TagList
            {
                { "exception.type", Normalize(exceptionType) },
                { "http.route", Normalize(route) }
            });
    }

    /// <summary>
    /// Records the throttle decision for a route.
    /// </summary>
    /// <param name="route">The request route.</param>
    /// <param name="allowed">Whether the request was allowed.</param>
    public static void RecordThrottleCheck(string? route, bool allowed)
    {
        var tags = new TagList { { "http.route", Normalize(route) } };
        if (allowed)
        {
            ThrottleAllowedCounter.Add(1, tags);
            return;
        }

        ThrottleRejectedCounter.Add(1, tags);
    }

    /// <summary>
    /// Records the device-code approval duration.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="durationSeconds">The approval duration in seconds.</param>
    public static void RecordDeviceCodeApprovalDuration(string? clientId, double durationSeconds)
    {
        DeviceCodeApprovalDurationHistogram.Record(
            durationSeconds,
            new TagList { { "dotauth.client_id", Normalize(clientId) } });
    }

    /// <summary>
    /// Records the duration of a token-store operation.
    /// </summary>
    /// <param name="operation">The token-store operation name.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <param name="hit">Whether a lookup operation found a stored token.</param>
    public static void RecordTokenStoreOperationDuration(string operation, double durationMs, bool? hit = null)
    {
        var tags = new TagList { { "dotauth.token_store.operation", Normalize(operation) } };
        if (hit is not null)
        {
            tags.Add("dotauth.token_store.hit", hit.Value);
        }

        TokenStoreOperationDurationHistogram.Record(durationMs, tags);
    }

    /// <summary>
    /// Normalizes unbounded or missing tag values into a stable form.
    /// </summary>
    /// <param name="value">The raw tag value.</param>
    /// <returns>A normalized tag value.</returns>
    public static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value;
    }

    /// <summary>
    /// Maps a token endpoint authentication method to a stable telemetry tag.
    /// </summary>
    /// <param name="authMethod">The raw authentication method.</param>
    /// <returns>The telemetry tag value.</returns>
    public static string MapClientAuthenticationMethod(string? authMethod)
    {
        return authMethod switch
        {
            TokenEndPointAuthenticationMethods.ClientSecretBasic => "client_secret_basic",
            TokenEndPointAuthenticationMethods.ClientSecretPost => "client_secret_post",
            TokenEndPointAuthenticationMethods.ClientSecretJwt => "client_secret_jwt",
            TokenEndPointAuthenticationMethods.PrivateKeyJwt => "private_key_jwt",
            TokenEndPointAuthenticationMethods.TlsClientAuth => "tls_client_auth",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Builds the common token tags shared by several instruments.
    /// </summary>
    /// <param name="grantType">The OAuth grant type.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>The populated tag list.</returns>
    private static TagList BuildCommonTags(string? grantType, string? clientId)
    {
        return new TagList
        {
            { "dotauth.grant_type", Normalize(grantType) },
            { "dotauth.client_id", Normalize(clientId) }
        };
    }

    /// <summary>
    /// Builds the client authentication tags shared by authentication metrics.
    /// </summary>
    /// <param name="authMethod">The client authentication method.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>The populated tag list.</returns>
    private static TagList BuildClientAuthTags(string? authMethod, string? clientId)
    {
        return new TagList
        {
            { "dotauth.client.auth_method", Normalize(authMethod) },
            { "dotauth.client_id", Normalize(clientId) }
        };
    }
}


