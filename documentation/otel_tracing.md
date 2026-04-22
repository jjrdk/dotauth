# OpenTelemetry Metrics and Traces for DotAuth Token Server

This document proposes OpenTelemetry instrumentation for the DotAuth token server. The proposals are based on a review of
the key OAuth 2.0 / OpenID Connect / UMA flows implemented in the server, covering the token lifecycle, authorization
flows, client/resource owner authentication, introspection, revocation, device authorization, and error conditions.

The recommended package is `System.Diagnostics.DiagnosticSource` (built in to .NET) for traces/spans and
`System.Diagnostics.Metrics` (also built in) for metrics, with the `OpenTelemetry.Extensions.Hosting` family of NuGet
packages wiring everything to an OTLP exporter.

---

## 1. Suggested Activity Source and Meter Names

| Name | Description |
|---|---|
| `DotAuth.TokenServer` | Activity source for all distributed traces |
| `DotAuth.TokenServer.Metrics` | Meter for all custom metrics |

---

## 2. Distributed Traces (Spans)

Each span should carry a minimal, stable set of attributes on every operation. Additional attributes are listed
per-span where they add diagnostic value. All client identifiers and usernames should be treated as low-cardinality
keys (hash or truncate when necessary to avoid cardinality explosion).

### 2.1 Token Endpoint — `POST /token`

**Activity name:** `dotauth.token.request`

| Attribute | Value |
|---|---|
| `dotauth.grant_type` | `authorization_code` / `refresh_token` / `password` / `client_credentials` / `device_code` / `urn:ietf:params:oauth:grant-type:uma-ticket` |
| `dotauth.client_id` | client identifier (low-cardinality) |
| `http.response.status_code` | HTTP status code returned |
| `dotauth.token.success` | `true` / `false` |
| `dotauth.error.code` | OAuth error code on failure (e.g. `invalid_client`, `invalid_grant`) |

Child spans per grant type:

#### 2.1.1 Authorization Code Grant

**Activity name:** `dotauth.token.authorization_code`

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier |
| `dotauth.auth_code.valid` | `true` / `false` |
| `dotauth.auth_code.expired` | `true` / `false` |
| `dotauth.pkce.present` | whether code_verifier was provided |
| `dotauth.pkce.valid` | PKCE verification result |
| `dotauth.token.reused` | `true` if a cached valid token was returned, `false` if newly generated |

#### 2.1.2 Resource Owner Password Grant

**Activity name:** `dotauth.token.password`

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier |
| `dotauth.resource_owner.authenticated` | `true` / `false` |
| `dotauth.amr` | authentication method reference used |
| `dotauth.scope.requested` | space-separated requested scopes |
| `dotauth.scope.granted` | space-separated granted scopes |
| `dotauth.token.reused` | cached vs. newly generated |

#### 2.1.3 Client Credentials Grant

**Activity name:** `dotauth.token.client_credentials`

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier |
| `dotauth.scope.requested` | space-separated requested scopes |
| `dotauth.scope.granted` | space-separated granted scopes |
| `dotauth.token.reused` | cached vs. newly generated |

#### 2.1.4 Refresh Token Grant

**Activity name:** `dotauth.token.refresh`

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier |
| `dotauth.refresh_token.found` | whether a stored refresh token was located |
| `dotauth.refresh_token.client_match` | whether token was issued to same client |
| `dotauth.scope.granted` | scopes on the new token |

#### 2.1.5 Device Code Grant

**Activity name:** `dotauth.token.device_code`

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier |
| `dotauth.device_code.status` | `approved` / `pending` / `expired` / `slow_down` |
| `dotauth.device_code.poll_interval_seconds` | configured polling interval |

#### 2.1.6 UMA Ticket Grant

**Activity name:** `dotauth.token.uma_ticket`

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier |
| `dotauth.uma.ticket_id` | UMA ticket identifier |
| `dotauth.uma.ticket_found` | whether the ticket was located |
| `dotauth.uma.ticket_expired` | whether the ticket had expired |
| `dotauth.uma.authorization_result` | `authorized` / `request_submitted` / `not_authorized` |

---

### 2.2 Token Revocation Endpoint — `POST /token/revoke`

**Activity name:** `dotauth.token.revoke`

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier of the revoking party |
| `dotauth.revoke.token_type` | `access_token` / `refresh_token` |
| `dotauth.revoke.token_found` | `true` / `false` |
| `dotauth.revoke.client_match` | whether token was issued to the requesting client |
| `dotauth.revoke.success` | `true` / `false` |

---

### 2.3 Authorization Endpoint — `GET /authorization`

**Activity name:** `dotauth.authorization.request`

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier |
| `dotauth.flow` | `authorization_code` / `implicit` / `hybrid` |
| `dotauth.response_type` | raw response_type string |
| `dotauth.pkce.required` | whether PKCE is required for the client |
| `dotauth.prompt` | prompt parameter value |
| `dotauth.user.authenticated` | whether the resource owner had an active session |
| `dotauth.action_result` | `redirect_to_callback` / `redirect_to_authenticate` / `redirect_to_consent` / `bad_request` |

---

### 2.4 Token Introspection Endpoint — `POST /introspect`

**Activity name:** `dotauth.introspection.request`

| Attribute | Value |
|---|---|
| `dotauth.introspection.token_found` | `true` / `false` |
| `dotauth.introspection.token_active` | `true` / `false` |

---

### 2.5 UserInfo Endpoint — `GET /userinfo`

**Activity name:** `dotauth.userinfo.request`

| Attribute | Value |
|---|---|
| `dotauth.userinfo.token_source` | `header` / `body` / `query_string` |
| `dotauth.userinfo.token_valid` | `true` / `false` |

---

### 2.6 Device Authorization Endpoint — `POST /device_authorization`

**Activity name:** `dotauth.device_authorization.request`

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier |
| `dotauth.scope.requested` | space-separated scopes |
| `dotauth.device_authorization.success` | `true` / `false` |

---

### 2.7 Client Authentication — `AuthenticateClient`

**Activity name:** `dotauth.client.authenticate`

Emitted as a child span inside all token request spans wherever `AuthenticateClient.Authenticate` is called.

| Attribute | Value |
|---|---|
| `dotauth.client_id` | client identifier (if available before auth failure) |
| `dotauth.client.auth_method` | `client_secret_basic` / `client_secret_post` / `private_key_jwt` / `tls_client_auth` |
| `dotauth.client.auth_success` | `true` / `false` |

---

### 2.8 Resource Owner Authentication — `IAuthenticateResourceOwnerService`

**Activity name:** `dotauth.resource_owner.authenticate`

| Attribute | Value |
|---|---|
| `dotauth.amr` | authentication method reference |
| `dotauth.resource_owner.found` | `true` / `false` |
| `dotauth.resource_owner.2fa_required` | `true` / `false` |

---

### 2.9 JWK Store Operations

**Activity name:** `dotauth.jwks.get_signing_key` / `dotauth.jwks.get_encryption_key`

| Attribute | Value |
|---|---|
| `dotauth.jwks.key_id` | JWK key id (`kid`) when available |
| `dotauth.jwks.algorithm` | signing / encryption algorithm |

---

### 2.10 Token Store Operations

**Activity name:** `dotauth.token_store.get` / `dotauth.token_store.add` / `dotauth.token_store.remove`

| Attribute | Value |
|---|---|
| `dotauth.token_store.operation` | `get_access_token` / `get_refresh_token` / `get_valid_token` / `add` / `remove_access` / `remove_refresh` |
| `dotauth.token_store.hit` | `true` if a valid cached token was found (for get operations) |

---

### 2.11 Throttle / Rate-Limit Filter

**Activity name:** `dotauth.throttle.check`

| Attribute | Value |
|---|---|
| `dotauth.throttle.allowed` | `true` / `false` |
| `http.route` | the endpoint route being throttled |

---

### 2.12 Unhandled Exceptions (ExceptionHandlerMiddleware)

**Activity name:** `dotauth.exception`

| Attribute | Value |
|---|---|
| `exception.type` | exception type name |
| `exception.message` | exception message |
| `http.route` | request route when the exception occurred |

Mark span status as `Error` and record the exception on the span.

---

## 3. Metrics

### 3.1 Token Issuance

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.tokens.issued` | `{tokens}` | Total tokens issued, tagged by `grant_type` and `client_id` |
| Counter | `dotauth.tokens.reused` | `{tokens}` | Tokens served from cache (no new JWT generated), tagged by `grant_type` |
| Histogram | `dotauth.token.issuance.duration` | `ms` | End-to-end latency of the token endpoint, tagged by `grant_type` and `success` |
| Counter | `dotauth.tokens.issue.failures` | `{requests}` | Failed token issuance attempts, tagged by `grant_type` and `error_code` |

### 3.2 Token Revocation

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.tokens.revoked` | `{tokens}` | Total revocations, tagged by `token_type` (`access_token` / `refresh_token`) |
| Counter | `dotauth.tokens.revoke.failures` | `{requests}` | Failed revocation attempts, tagged by `error_code` |

### 3.3 Authorization Code Flow

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.authorization_codes.issued` | `{codes}` | Authorization codes issued |
| Counter | `dotauth.authorization_codes.redeemed` | `{codes}` | Authorization codes successfully exchanged for tokens |
| Counter | `dotauth.authorization_codes.expired` | `{codes}` | Authorization codes rejected as expired |
| Counter | `dotauth.authorization_codes.invalid` | `{codes}` | Codes rejected for other reasons (wrong client, bad PKCE, etc.) |

### 3.4 Refresh Tokens

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.refresh_tokens.used` | `{tokens}` | Successful refresh token grants |
| Counter | `dotauth.refresh_tokens.invalid` | `{tokens}` | Refresh tokens rejected (not found or wrong client) |

### 3.5 Device Authorization

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.device_authorization.started` | `{requests}` | Device authorization flows initiated |
| Counter | `dotauth.device_code.polls` | `{polls}` | Device code polling attempts, tagged by `status` (`approved` / `pending` / `expired` / `slow_down`) |
| Histogram | `dotauth.device_code.approval.duration` | `s` | Time from device code issuance to user approval |

### 3.6 UMA / RPT

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.uma.rpt.issued` | `{tokens}` | Requesting Party Tokens (RPTs) successfully issued |
| Counter | `dotauth.uma.rpt.request_submitted` | `{requests}` | UMA requests that required interactive authorization (need_info / request_submitted) |
| Counter | `dotauth.uma.rpt.denied` | `{requests}` | UMA requests denied by policy |
| Counter | `dotauth.uma.ticket.expired` | `{tickets}` | UMA ticket lookups that failed due to expiry |

### 3.7 Client Authentication

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.client.auth.success` | `{attempts}` | Successful client authentications, tagged by `auth_method` |
| Counter | `dotauth.client.auth.failure` | `{attempts}` | Failed client authentications, tagged by `auth_method` |

### 3.8 Resource Owner / User Authentication

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.resource_owner.auth.success` | `{attempts}` | Successful user authentications, tagged by `amr` |
| Counter | `dotauth.resource_owner.auth.failure` | `{attempts}` | Failed user authentications |
| Counter | `dotauth.resource_owner.2fa.initiated` | `{flows}` | Two-factor authentication flows started |

### 3.9 Introspection and UserInfo

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.introspection.requests` | `{requests}` | Total introspection requests |
| Counter | `dotauth.introspection.active` | `{tokens}` | Introspection responses returning `active: true` |
| Counter | `dotauth.introspection.inactive` | `{tokens}` | Introspection responses returning `active: false` / token not found |
| Counter | `dotauth.userinfo.requests` | `{requests}` | Total userinfo requests, tagged by `token_valid` |

### 3.10 Rate Limiting

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.throttle.allowed` | `{requests}` | Requests allowed by the throttle filter, tagged by `route` |
| Counter | `dotauth.throttle.rejected` | `{requests}` | Requests rejected (HTTP 429) by the throttle filter, tagged by `route` |

### 3.11 Error and Exception Rates

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Counter | `dotauth.errors.oauth` | `{errors}` | OAuth protocol errors returned to clients, tagged by `error_code` |
| Counter | `dotauth.errors.unhandled` | `{errors}` | Unhandled exceptions caught by `ExceptionHandlerMiddleware`, tagged by `exception_type` |

### 3.12 Token Store Performance

| Instrument | Name | Unit | Description |
|---|---|---|---|
| Histogram | `dotauth.token_store.operation.duration` | `ms` | Latency of token store operations, tagged by `operation` |
| UpDownCounter | `dotauth.token_store.active_tokens` | `{tokens}` | Estimated number of live tokens in the store (requires store implementation support) |

---

## 4. Recommended Tags / Attribute Conventions

To keep cardinality manageable while still enabling useful aggregations, all instruments and spans should use the
following conventions:

- `dotauth.grant_type` — OAuth grant type string (low-cardinality enum value)
- `dotauth.client_id` — OAuth client identifier; hash if values are user-generated or unbounded
- `dotauth.error_code` — OAuth error code string (e.g. `invalid_grant`, `invalid_client`)
- `dotauth.amr` — authentication method reference (e.g. `pwd`, `otp`, `sms`)
- `http.route` — ASP.NET Core route template (e.g. `/connect/token`)
- Standard OpenTelemetry HTTP semantic conventions (`http.request.method`, `http.response.status_code`, `server.address`)

---

## 5. Integration Points

The following existing DotAuth extension points are the natural places to inject OTel instrumentation:

| Extension Point | Instrumentation Approach |
|---|---|
| `IEventPublisher` | Implement an `OtelEventPublisher` that converts `TokenGranted`, `TokenRevoked`, `AuthorizationGranted`, `RptIssued`, `DotAuthError`, etc. events into metric increments and span events |
| `TokenController.PostToken` / `PostRevoke` | Start/stop `dotauth.token.request` and `dotauth.token.revoke` activities using `ActivitySource.StartActivity` |
| `AuthorizationController.Get` | Start/stop `dotauth.authorization.request` activity |
| `IntrospectionController.Post` | Start/stop `dotauth.introspection.request` activity |
| `ThrottleFilter.OnResourceExecutionAsync` | Record throttle allow/reject metrics and set span attributes |
| `ExceptionHandlerMiddleware.Invoke` | Record `dotauth.errors.unhandled` counter and mark span as error |
| `AuthenticateClient.Authenticate` | Start/stop `dotauth.client.authenticate` child span |
| Store interface implementations (`ITokenStore`, `IJwksStore`) | Wrap with decorator to record store latency histograms |

---

## 6. Suggested Alerts Based on These Metrics

| Alert | Condition |
|---|---|
| High token issuance failure rate | `dotauth.tokens.issue.failures` rate > X% of `dotauth.tokens.issued` over 5 minutes |
| Spike in invalid client authentications | `dotauth.client.auth.failure` rate sustained above baseline — possible credential stuffing |
| Throttle rejections increasing | `dotauth.throttle.rejected` rate rising — possible abuse or misconfigured client |
| Unhandled exceptions | Any non-zero `dotauth.errors.unhandled` should page |
| Device code slow-down events | High `dotauth.device_code.polls{status=slow_down}` — client polling too aggressively |
| UMA policy denials | Sustained `dotauth.uma.rpt.denied` — possible policy misconfiguration or attack |
| Token revocation failure | `dotauth.tokens.revoke.failures` rising — token store issues |

