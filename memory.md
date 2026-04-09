# Memory

## Current Tasks
- No active tasks.

## Working Context
- Existing telemetry support already includes custom `ActivitySource`/`Meter` registration in `src/dotauth/ServiceCollectionExtensions.cs` and initial spans/metrics for token, revocation, authorization, device authorization, introspection, userinfo, and client authentication paths.
- Remaining likely gaps compared to `otel_tracing.md`: richer token-grant spans for authorization code, password, and UMA ticket flows; additional counters for reuse/failures/UMA/error/throttle/exception outcomes; throttle and exception instrumentation; and README documentation.
- `src/dotauth/Extensions/ResourceOwnerAuthenticateHelper.cs` already emits `dotauth.resource_owner.authenticate` spans and success/failure counters, so the password grant work should build on those existing hooks instead of duplicating them.
- Acceptance tests live in `tests/dotauth.acceptancetests` and use Reqnroll with partial `FeatureTest` step definition files.
- Existing tests already cover client credentials, refresh, revocation, device authorization, UMA, authorization code, user info, and introspection flows.
- The new telemetry tests are now present as a test-first slice in `Features/OpenTelemetryInstrumentation.feature` with supporting step definitions and collector support.
- The acceptance-test project builds successfully after adding Testcontainers and the collector support code.
- Current implementation goal was completed by extending the existing telemetry while preserving server behavior.
- Completed and verified:
  - client credentials success telemetry scenario
  - invalid client failure telemetry scenario
  - refresh + revoke telemetry scenario
  - authorization request telemetry scenario
  - device authorization telemetry scenario
  - userinfo + introspection telemetry scenario
  - password grant telemetry scenario
  - throttle and exception telemetry unit coverage
  - telemetry-decorated token store and JWK store registrations
  - `README.md` OpenTelemetry documentation update


