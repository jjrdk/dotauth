# Lessons Learned

## Acceptance testing OpenTelemetry in DotAuth

- Reqnroll acceptance tests in `tests/dotauth.acceptancetests` are organized as partial `FeatureTest` classes, so new feature-specific steps can be added without changing unrelated scenarios.
- For telemetry test-first work, an OpenTelemetry Collector container is a good seam even before server instrumentation exists:
  - it gives the tests a real OTLP endpoint to target later,
  - it allows deterministic exporter configuration,
  - and it keeps telemetry assertions external to the server implementation.
- When asserting custom metric names in acceptance tests, exporting metrics to a file from the collector is easier than scraping Prometheus-normalized names because the file exporter preserves the original instrument names.
- The OpenTelemetry .NET OTLP HTTP exporter must target signal-specific collector paths:
  - traces -> `/v1/traces`
  - metrics -> `/v1/metrics`
  Pointing the exporter at the collector root URL causes repeated `404` responses even though the collector is reachable.
- The acceptance tests exercise `UmaIntrospectionController` for RPT inspection, so the `dotauth.introspection.request` telemetry needs to be emitted from both the standard OAuth introspection endpoint and the UMA introspection endpoint.
- Reading collector logs for assertions should use a bounded time window. Asking Docker for logs through `DateTime.MaxValue` can behave like a streaming tail and make tests hang.
- The safest place to add infrastructure telemetry around `ITokenStore` and `IJwksRepository` is the existing dependency-registration factory in `ServiceCollectionExtensions`. Wrapping the configured implementation there preserves the public configuration delegates while enabling instrumentation only when OTLP export is configured.
- The acceptance-test client for password-grant scenarios is the `client/client` resource-owner flow client, not the `clientCredentials/clientCredentials` client. Reusing the existing resource-owner client avoids false telemetry failures caused by `invalid_scope` responses.

