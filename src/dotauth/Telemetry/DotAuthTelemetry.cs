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
    /// Centralizes custom activity names so callers avoid typos.
    /// </summary>
    public static class ActivityNames
    {
        public const string AuthorizationRequest = "dotauth.authorization.request";
        public const string ClientAuthenticate = "dotauth.client.authenticate";
        public const string DeviceAuthorizationRequest = "dotauth.device_authorization.request";
        public const string Exception = "dotauth.exception";
        public const string IntrospectionRequest = "dotauth.introspection.request";
        public const string JwksGetEncryptionKey = "dotauth.jwks.get_encryption_key";
        public const string JwksGetPublicKeys = "dotauth.jwks.get_public_keys";
        public const string JwksGetSigningKey = "dotauth.jwks.get_signing_key";
        public const string ResourceOwnerAuthenticate = "dotauth.resource_owner.authenticate";
        public const string ThrottleCheck = "dotauth.throttle.check";
        public const string TokenAuthorizationCode = "dotauth.token.authorization_code";
        public const string TokenClientCredentials = "dotauth.token.client_credentials";
        public const string TokenDeviceCode = "dotauth.token.device_code";
        public const string TokenPassword = "dotauth.token.password";
        public const string TokenRefresh = "dotauth.token.refresh";
        public const string TokenRequest = "dotauth.token.request";
        public const string TokenRevoke = "dotauth.token.revoke";
        public const string TokenStoreAdd = "dotauth.token_store.add";
        public const string TokenStoreGet = "dotauth.token_store.get";
        public const string TokenStoreRemove = "dotauth.token_store.remove";
        public const string TokenUmaTicket = "dotauth.token.uma_ticket";
        public const string UserInfoRequest = "dotauth.userinfo.request";
    }

    /// <summary>
    /// Centralizes custom metric names so callers avoid typos.
    /// </summary>
    public static class MetricNames
    {
        public const string AuthorizationCodesExpired = "dotauth.authorization_codes.expired";
        public const string AuthorizationCodesInvalid = "dotauth.authorization_codes.invalid";
        public const string AuthorizationCodesIssued = "dotauth.authorization_codes.issued";
        public const string AuthorizationCodesRedeemed = "dotauth.authorization_codes.redeemed";
        public const string ClientAuthFailure = "dotauth.client.auth.failure";
        public const string ClientAuthSuccess = "dotauth.client.auth.success";
        public const string DeviceAuthorizationStarted = "dotauth.device_authorization.started";
        public const string DeviceCodeApprovalDuration = "dotauth.device_code.approval.duration";
        public const string DeviceCodePolls = "dotauth.device_code.polls";
        public const string IntrospectionActive = "dotauth.introspection.active";
        public const string IntrospectionInactive = "dotauth.introspection.inactive";
        public const string IntrospectionRequests = "dotauth.introspection.requests";
        public const string OAuthErrors = "dotauth.errors.oauth";
        public const string RefreshTokensInvalid = "dotauth.refresh_tokens.invalid";
        public const string RefreshTokensUsed = "dotauth.refresh_tokens.used";
        public const string ResourceOwnerAuthFailure = "dotauth.resource_owner.auth.failure";
        public const string ResourceOwnerAuthSuccess = "dotauth.resource_owner.auth.success";
        public const string ThrottleAllowed = "dotauth.throttle.allowed";
        public const string ThrottleRejected = "dotauth.throttle.rejected";
        public const string TokenIssueFailures = "dotauth.tokens.issue.failures";
        public const string TokenIssuanceDuration = "dotauth.token.issuance.duration";
        public const string TokenRevokeFailures = "dotauth.tokens.revoke.failures";
        public const string TokenStoreOperationDuration = "dotauth.token_store.operation.duration";
        public const string TokensIssued = "dotauth.tokens.issued";
        public const string TokensRevoked = "dotauth.tokens.revoked";
        public const string TokensReused = "dotauth.tokens.reused";
        public const string UmaRptDenied = "dotauth.uma.rpt.denied";
        public const string UmaRptIssued = "dotauth.uma.rpt.issued";
        public const string UmaRptRequestSubmitted = "dotauth.uma.rpt.request_submitted";
        public const string UmaTicketExpired = "dotauth.uma.ticket.expired";
        public const string UnhandledErrors = "dotauth.errors.unhandled";
        public const string UserInfoRequests = "dotauth.userinfo.requests";
    }

    /// <summary>
    /// Centralizes custom and standard telemetry tag keys so callers avoid typos.
    /// </summary>
    public static class TagKeys
    {
        public const string Amr = "dotauth.amr";
        public const string ClientId = "dotauth.client_id";
        public const string ErrorCode = "dotauth.error_code";
        public const string ExceptionMessage = "exception.message";
        public const string ExceptionType = "exception.type";
        public const string GrantType = "dotauth.grant_type";
        public const string HttpRoute = "http.route";
        public const string ResponseType = "dotauth.response_type";
        public const string ScopeGranted = "dotauth.scope.granted";
        public const string ScopeRequested = "dotauth.scope.requested";
        public const string Success = "success";
        public const string Status = "status";
        public const string TokenReused = "dotauth.token.reused";
        public const string TokenTypeHint = "dotauth.token_type_hint";
        public const string UserInfoTokenValid = "dotauth.userinfo.token_valid";

        public const string AuthCodeExpired = "dotauth.auth_code.expired";
        public const string AuthCodeValid = "dotauth.auth_code.valid";

        public const string ClientAuthMethod = "dotauth.client.auth_method";
        public const string ClientAuthSuccess = "dotauth.client.auth_success";

        public const string DeviceCodePollIntervalSeconds = "dotauth.device_code.poll_interval_seconds";
        public const string DeviceCodeStatus = "dotauth.device_code.status";

        public const string IntrospectionTokenActive = "dotauth.introspection.token_active";
        public const string IntrospectionTokenFound = "dotauth.introspection.token_found";

        public const string JwksAlgorithm = "dotauth.jwks.algorithm";
        public const string JwksKeyCount = "dotauth.jwks.key_count";
        public const string JwksKeyFound = "dotauth.jwks.key_found";
        public const string JwksKeyId = "dotauth.jwks.key_id";

        public const string PkcePresent = "dotauth.pkce.present";
        public const string PkceValid = "dotauth.pkce.valid";

        public const string RefreshTokenClientMatch = "dotauth.refresh_token.client_match";
        public const string RefreshTokenFound = "dotauth.refresh_token.found";

        public const string ResourceOwner2FaRequired = "dotauth.resource_owner.2fa_required";
        public const string ResourceOwnerAuthenticated = "dotauth.resource_owner.authenticated";
        public const string ResourceOwnerFound = "dotauth.resource_owner.found";

        public const string RevokeTokenType = "dotauth.revoke.token_type";

        public const string ThrottleAllowed = "dotauth.throttle.allowed";

        public const string TokenStoreHit = "dotauth.token_store.hit";
        public const string TokenStoreOperation = "dotauth.token_store.operation";
        public const string TokenStoreSuccess = "dotauth.token_store.success";

        public const string UmaAuthorizationResult = "dotauth.uma.authorization_result";
        public const string UmaTicketExpired = "dotauth.uma.ticket_expired";
        public const string UmaTicketFound = "dotauth.uma.ticket_found";
        public const string UmaTicketId = "dotauth.uma.ticket_id";
    }

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
        MetricNames.TokensIssued,
        unit: "{tokens}",
        description: "Counts successful token responses.");
    private static readonly Counter<long> TokensReusedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.TokensReused,
        unit: "{tokens}",
        description: "Counts token responses that reused an existing token.");
    private static readonly Histogram<double> TokenIssuanceDurationHistogram = MeterInstance.CreateHistogram<double>(
        MetricNames.TokenIssuanceDuration,
        unit: "ms",
        description: "Measures token endpoint duration.");
    private static readonly Counter<long> TokenIssueFailuresCounter = MeterInstance.CreateCounter<long>(
        MetricNames.TokenIssueFailures,
        unit: "{requests}",
        description: "Counts failed token responses.");
    private static readonly Counter<long> TokensRevokedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.TokensRevoked,
        unit: "{tokens}",
        description: "Counts successful token revocations.");
    private static readonly Counter<long> TokenRevokeFailuresCounter = MeterInstance.CreateCounter<long>(
        MetricNames.TokenRevokeFailures,
        unit: "{requests}",
        description: "Counts failed token revocation attempts.");
    private static readonly Counter<long> RefreshTokensUsedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.RefreshTokensUsed,
        unit: "{tokens}",
        description: "Counts successful refresh-token grants.");
    private static readonly Counter<long> RefreshTokensInvalidCounter = MeterInstance.CreateCounter<long>(
        MetricNames.RefreshTokensInvalid,
        unit: "{tokens}",
        description: "Counts invalid refresh-token grant attempts.");
    private static readonly Counter<long> DeviceAuthorizationStartedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.DeviceAuthorizationStarted,
        unit: "{requests}",
        description: "Counts started device authorization flows.");
    private static readonly Counter<long> DeviceCodePollsCounter = MeterInstance.CreateCounter<long>(
        MetricNames.DeviceCodePolls,
        unit: "{polls}",
        description: "Counts device code polling attempts.");
    private static readonly Counter<long> AuthorizationCodesIssuedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.AuthorizationCodesIssued,
        unit: "{codes}",
        description: "Counts authorization code flow requests that advance authorization code issuance.");
    private static readonly Counter<long> AuthorizationCodesRedeemedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.AuthorizationCodesRedeemed,
        unit: "{codes}",
        description: "Counts authorization codes successfully exchanged for tokens.");
    private static readonly Counter<long> AuthorizationCodesExpiredCounter = MeterInstance.CreateCounter<long>(
        MetricNames.AuthorizationCodesExpired,
        unit: "{codes}",
        description: "Counts authorization codes rejected because they expired.");
    private static readonly Counter<long> AuthorizationCodesInvalidCounter = MeterInstance.CreateCounter<long>(
        MetricNames.AuthorizationCodesInvalid,
        unit: "{codes}",
        description: "Counts authorization codes rejected for non-expiry reasons.");
    private static readonly Counter<long> IntrospectionRequestsCounter = MeterInstance.CreateCounter<long>(
        MetricNames.IntrospectionRequests,
        unit: "{requests}",
        description: "Counts token introspection requests.");
    private static readonly Counter<long> IntrospectionActiveCounter = MeterInstance.CreateCounter<long>(
        MetricNames.IntrospectionActive,
        unit: "{tokens}",
        description: "Counts introspection responses that returned active tokens.");
    private static readonly Counter<long> IntrospectionInactiveCounter = MeterInstance.CreateCounter<long>(
        MetricNames.IntrospectionInactive,
        unit: "{tokens}",
        description: "Counts introspection responses that returned inactive or missing tokens.");
    private static readonly Counter<long> UserInfoRequestsCounter = MeterInstance.CreateCounter<long>(
        MetricNames.UserInfoRequests,
        unit: "{requests}",
        description: "Counts user info requests.");
    private static readonly Counter<long> ClientAuthSuccessCounter = MeterInstance.CreateCounter<long>(
        MetricNames.ClientAuthSuccess,
        unit: "{attempts}",
        description: "Counts successful client authentication attempts.");
    private static readonly Counter<long> ClientAuthFailureCounter = MeterInstance.CreateCounter<long>(
        MetricNames.ClientAuthFailure,
        unit: "{attempts}",
        description: "Counts failed client authentication attempts.");
    private static readonly Counter<long> ResourceOwnerAuthSuccessCounter = MeterInstance.CreateCounter<long>(
        MetricNames.ResourceOwnerAuthSuccess,
        unit: "{attempts}",
        description: "Counts successful resource-owner authentications.");
    private static readonly Counter<long> ResourceOwnerAuthFailureCounter = MeterInstance.CreateCounter<long>(
        MetricNames.ResourceOwnerAuthFailure,
        unit: "{attempts}",
        description: "Counts failed resource-owner authentications.");
    private static readonly Counter<long> UmaRptIssuedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.UmaRptIssued,
        unit: "{tokens}",
        description: "Counts issued UMA requesting-party tokens.");
    private static readonly Counter<long> UmaRptRequestSubmittedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.UmaRptRequestSubmitted,
        unit: "{requests}",
        description: "Counts UMA requests that require interactive authorization.");
    private static readonly Counter<long> UmaRptDeniedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.UmaRptDenied,
        unit: "{requests}",
        description: "Counts UMA requests denied by policy.");
    private static readonly Counter<long> UmaTicketExpiredCounter = MeterInstance.CreateCounter<long>(
        MetricNames.UmaTicketExpired,
        unit: "{tickets}",
        description: "Counts UMA ticket lookups that failed because the ticket expired.");
    private static readonly Counter<long> OAuthErrorsCounter = MeterInstance.CreateCounter<long>(
        MetricNames.OAuthErrors,
        unit: "{errors}",
        description: "Counts OAuth protocol errors returned to callers.");
    private static readonly Counter<long> UnhandledErrorsCounter = MeterInstance.CreateCounter<long>(
        MetricNames.UnhandledErrors,
        unit: "{errors}",
        description: "Counts unhandled exceptions captured by middleware.");
    private static readonly Counter<long> ThrottleAllowedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.ThrottleAllowed,
        unit: "{requests}",
        description: "Counts requests allowed by the throttle filter.");
    private static readonly Counter<long> ThrottleRejectedCounter = MeterInstance.CreateCounter<long>(
        MetricNames.ThrottleRejected,
        unit: "{requests}",
        description: "Counts requests rejected by the throttle filter.");
    private static readonly Histogram<double> DeviceCodeApprovalDurationHistogram = MeterInstance.CreateHistogram<double>(
        MetricNames.DeviceCodeApprovalDuration,
        unit: "s",
        description: "Measures time from device authorization issuance to approval.");
    private static readonly Histogram<double> TokenStoreOperationDurationHistogram = MeterInstance.CreateHistogram<double>(
        MetricNames.TokenStoreOperationDuration,
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
        tags.Add(TagKeys.ErrorCode, Normalize(errorCode));
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
        tags.Add(TagKeys.Success, success);
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
            { TagKeys.RevokeTokenType, Normalize(tokenType) },
            { TagKeys.ClientId, Normalize(clientId) }
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
            { TagKeys.RevokeTokenType, Normalize(tokenType) },
            { TagKeys.ClientId, Normalize(clientId) },
            { TagKeys.ErrorCode, Normalize(errorCode) }
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
        RefreshTokensUsedCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
    }

    /// <summary>
    /// Records an invalid refresh token attempt.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordRefreshTokenInvalid(string? clientId)
    {
        RefreshTokensInvalidCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
    }

    /// <summary>
    /// Records the start of a device authorization flow.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordDeviceAuthorizationStarted(string? clientId)
    {
        DeviceAuthorizationStartedCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
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
                { TagKeys.ClientId, Normalize(clientId) },
                { TagKeys.Status, Normalize(status) }
            });
    }

    /// <summary>
    /// Records authorization code flow activity for authorization requests.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordAuthorizationCodeIssued(string? clientId)
    {
        AuthorizationCodesIssuedCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
    }

    /// <summary>
    /// Records a successful authorization code redemption.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordAuthorizationCodeRedeemed(string? clientId)
    {
        AuthorizationCodesRedeemedCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
    }

    /// <summary>
    /// Records an expired authorization code.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordAuthorizationCodeExpired(string? clientId)
    {
        AuthorizationCodesExpiredCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
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
                { TagKeys.ClientId, Normalize(clientId) },
                { TagKeys.ErrorCode, Normalize(errorCode) }
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
            { TagKeys.IntrospectionTokenFound, tokenFound },
            { TagKeys.IntrospectionTokenActive, tokenActive }
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
        UserInfoRequestsCounter.Add(1, new TagList { { TagKeys.UserInfoTokenValid, tokenValid } });
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
        ResourceOwnerAuthSuccessCounter.Add(1, new TagList { { TagKeys.Amr, Normalize(amr) } });
    }

    /// <summary>
    /// Records a failed resource-owner authentication.
    /// </summary>
    /// <param name="amr">The authentication method reference.</param>
    public static void RecordResourceOwnerAuthenticationFailure(string? amr)
    {
        ResourceOwnerAuthFailureCounter.Add(1, new TagList { { TagKeys.Amr, Normalize(amr) } });
    }

    /// <summary>
    /// Records a successfully issued UMA requesting-party token.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordUmaRptIssued(string? clientId)
    {
        UmaRptIssuedCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
    }

    /// <summary>
    /// Records a UMA request that was submitted for later authorization.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordUmaRptRequestSubmitted(string? clientId)
    {
        UmaRptRequestSubmittedCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
    }

    /// <summary>
    /// Records a UMA request denied by policy.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordUmaRptDenied(string? clientId)
    {
        UmaRptDeniedCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
    }

    /// <summary>
    /// Records an expired UMA ticket.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    public static void RecordUmaTicketExpired(string? clientId)
    {
        UmaTicketExpiredCounter.Add(1, new TagList { { TagKeys.ClientId, Normalize(clientId) } });
    }

    /// <summary>
    /// Records an OAuth error response.
    /// </summary>
    /// <param name="errorCode">The OAuth error code.</param>
    public static void RecordOAuthError(string? errorCode)
    {
        OAuthErrorsCounter.Add(1, new TagList { { TagKeys.ErrorCode, Normalize(errorCode) } });
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
                { TagKeys.ExceptionType, Normalize(exceptionType) },
                { TagKeys.HttpRoute, Normalize(route) }
            });
    }

    /// <summary>
    /// Records the throttle decision for a route.
    /// </summary>
    /// <param name="route">The request route.</param>
    /// <param name="allowed">Whether the request was allowed.</param>
    public static void RecordThrottleCheck(string? route, bool allowed)
    {
        var tags = new TagList { { TagKeys.HttpRoute, Normalize(route) } };
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
            new TagList { { TagKeys.ClientId, Normalize(clientId) } });
    }

    /// <summary>
    /// Records the duration of a token-store operation.
    /// </summary>
    /// <param name="operation">The token-store operation name.</param>
    /// <param name="durationMs">The operation duration in milliseconds.</param>
    /// <param name="hit">Whether a lookup operation found a stored token.</param>
    public static void RecordTokenStoreOperationDuration(string operation, double durationMs, bool? hit = null)
    {
        var tags = new TagList { { TagKeys.TokenStoreOperation, Normalize(operation) } };
        if (hit is not null)
        {
            tags.Add(TagKeys.TokenStoreHit, hit.Value);
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
            { TagKeys.GrantType, Normalize(grantType) },
            { TagKeys.ClientId, Normalize(clientId) }
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
            { TagKeys.ClientAuthMethod, Normalize(authMethod) },
            { TagKeys.ClientId, Normalize(clientId) }
        };
    }
}


