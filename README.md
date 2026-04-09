# Dot Auth

![GitHub CI](https://github.com/jjrdk/dotauth/actions/workflows/github.yml/badge.svg)

## Description

DotAuth is an authorization server SDK. The simplest way consume it is as a ready build container at [Docker Hub](https://hub.docker.com/r/jjrdk/dotauth).

You can also use the SDK to create an authorization server and add more custom configurations. See the different [server examples](https://github.com/jjrdk/DotAuth/tree/master/src) on how to get started.

## Origin

DotAuth is based on the [SimpleIdentityServer](https://github.com/thabart/SimpleIdentityServer) project, but has been reduced and adjusted to make it more cloud friendly.

Most features have been merged into the dotauth project.

All EntityFramework dependencies have been stripped away. It is up to you to provide your own implementation of repositories.

## Runtime Environment

The project runs under .NET 6 & .NET 7.

This project has been tested to run in Docker, AWS Lambda and as a simple process, in both Windows and Linux.

## Supported Protocols

Supports OpenID Connect (OIDC), OAuth2 and UMA standards.

The support for SCIM has been removed.

## Building the Project

To build the project, run the build script (build.ps1 on Windows, build.sh on Linux/Mac). This will generate a set of nuget packages which can be used to integrate DotAuth into an ASP.NET Core server project.

See the example [Auth Server project](https://github.com/jjrdk/DotAuth/tree/master/src/dotauth.authserver) for an example of how to use DotAuth as an auth server.

## OpenTelemetry traces and metrics

DotAuth can emit custom OpenTelemetry traces and metrics for the main OAuth2, OpenID Connect, and UMA server flows.

Telemetry export is enabled only when `OTEL_EXPORTER_OTLP_ENDPOINT` is set. When that environment variable is missing, DotAuth runs without registering the custom OpenTelemetry pipeline.

### Environment variables

|Environment Variable|Type|Description|
|---|---|---|
|`OTEL_EXPORTER_OTLP_ENDPOINT`|url string|Required to enable OpenTelemetry export. Use the collector base URL, for example `http://localhost:4318/`.|
|`OTEL_EXPORTER_OTLP_PROTOCOL`|string|Optional. Set to `http/protobuf` for OTLP/HTTP export. If omitted, DotAuth uses OTLP/gRPC.|
|`OTEL_EXPORTER_OTLP_HEADERS`|string|Optional. Standard OTLP header list used when exporting through an authenticated proxy, for example `authorization=Basic <base64-value>`.|
|`OTEL_SERVICE_NAME`|string|Optional standard OpenTelemetry resource attribute used by downstream collectors/backends.|
|`DOTAUTH_OTEL_CLIENT_CERTIFICATE_PATH`|path string|Optional. Path to a client certificate in PKCS#12 (`.pfx`) format when the collector requires mTLS.|
|`DOTAUTH_OTEL_CLIENT_CERTIFICATE_PASSWORD`|string|Optional password for `DOTAUTH_OTEL_CLIENT_CERTIFICATE_PATH`.|

When `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`, DotAuth automatically appends the signal-specific OTLP paths expected by the collector:

- traces → `/v1/traces`
- metrics → `/v1/metrics`

### Local collector and backend example

For local development, a small OpenTelemetry Collector plus a trace backend is enough to inspect DotAuth telemetry end-to-end.
The example below keeps traces in Jaeger and exposes metrics from the collector in Prometheus format on `:9464`.

A checked-in collector example is available at [`otel-local/collector.yaml`](otel-local/collector.yaml).
It accepts OTLP over gRPC and HTTP, forwards traces to Jaeger, and exposes metrics in Prometheus format for local scraping.
This file is intentionally a local-only convenience example and should not be exposed on shared networks.

If you want a simple local backend stack, the following `docker-compose.yml` snippet is enough for development. Use pinned image tags in your own environment as needed:

```yaml
services:
  jaeger:
    image: jaegertracing/all-in-one
    ports:
      - "16686:16686"

  otel-collector:
    image: otel/opentelemetry-collector-contrib
    command: ["--config=/etc/otelcol-contrib/config.yaml"]
    volumes:
      - ./otel-local/collector.yaml:/etc/otelcol-contrib/config.yaml:ro
    ports:
      - "127.0.0.1:4317:4317"
      - "127.0.0.1:4318:4318"
      - "127.0.0.1:9464:9464"
    depends_on:
      - jaeger
```

Point DotAuth to the collector with standard OTLP environment variables:

```zsh
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318/"
export OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf"
export OTEL_SERVICE_NAME="dotauth-local"
```

With that setup in place:

- open Jaeger at `http://localhost:16686` to inspect traces
- scrape `http://localhost:9464/metrics` from Prometheus or view it directly during development
- use `otel_tracing.md` as the detailed reference for the span names, metric names, and recommended tags emitted by DotAuth

### Protected collector ingestion for shared environments

When the OTLP endpoint is reachable by anything beyond the local developer machine, require either mTLS or an authenticated proxy.

Checked-in hardened examples:

- direct collector mTLS: [`otel-secure/collector.mtls.yaml`](otel-secure/collector.mtls.yaml)
- collector behind an authenticated proxy: [`otel-secure/collector.proxy-backend.yaml`](otel-secure/collector.proxy-backend.yaml) and [`otel-secure/nginx.conf`](otel-secure/nginx.conf)

#### Option 1: Require mTLS at the collector

`otel-secure/collector.mtls.yaml` configures both OTLP/gRPC and OTLP/HTTP receivers to require a client certificate signed by the configured CA.
DotAuth can present a client certificate to that collector by setting these environment variables:

```zsh
export OTEL_EXPORTER_OTLP_ENDPOINT="https://otel-collector.internal:4318/"
export OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf"
export DOTAUTH_OTEL_CLIENT_CERTIFICATE_PATH="/secure/path/dotauth-otel-client.pfx"
export DOTAUTH_OTEL_CLIENT_CERTIFICATE_PASSWORD="change-me"
```

Use a server certificate chain trusted by the DotAuth host, or import your internal CA into the machine trust store before enabling export.

#### Option 2: Put OTLP behind an authenticated proxy

`otel-secure/nginx.conf` shows a simple TLS-terminating reverse proxy that requires authentication before forwarding `/v1/traces` and `/v1/metrics` to the collector.
DotAuth can send proxy credentials with the standard OTLP headers environment variable:

```zsh
export OTEL_EXPORTER_OTLP_ENDPOINT="https://otel-proxy.internal:4318/"
export OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf"
export OTEL_EXPORTER_OTLP_HEADERS="authorization=Basic <base64-user-colon-password>"
```

If your proxy expects a bearer token instead of basic authentication, use the same header mechanism with `authorization=Bearer <token>`.

### DotAuth telemetry names

|Type|Name|
|---|---|
|Activity source|`DotAuth.TokenServer`|
|Meter|`DotAuth.TokenServer.Metrics`|

### Emitted trace spans

DotAuth emits custom spans for the main request and sub-operation boundaries, including:

- token endpoint requests: `dotauth.token.request`
- grant-specific token flows: `dotauth.token.client_credentials`, `dotauth.token.password`, `dotauth.token.authorization_code`, `dotauth.token.refresh`, `dotauth.token.device_code`, `dotauth.token.uma_ticket`
- token revocation: `dotauth.token.revoke`
- client and resource-owner authentication: `dotauth.client.authenticate`, `dotauth.resource_owner.authenticate`
- authorization, introspection, and user info endpoints: `dotauth.authorization.request`, `dotauth.introspection.request`, `dotauth.userinfo.request`
- device authorization: `dotauth.device_authorization.request`
- throttling and exception handling: `dotauth.throttle.check`, `dotauth.exception`
- supporting store operations: `dotauth.token_store.get`, `dotauth.token_store.add`, `dotauth.token_store.remove`, `dotauth.jwks.get_signing_key`, `dotauth.jwks.get_encryption_key`, `dotauth.jwks.get_public_keys`

These spans are tagged with stable attributes such as grant type, client id, response status, OAuth error code, token reuse, UMA authorization result, route, and cache/store hit indicators.

### Emitted metrics

DotAuth also emits counters and histograms for the most important server outcomes, including:

- token lifecycle: `dotauth.tokens.issued`, `dotauth.tokens.reused`, `dotauth.tokens.issue.failures`, `dotauth.token.issuance.duration`
- revocation and refresh: `dotauth.tokens.revoked`, `dotauth.tokens.revoke.failures`, `dotauth.refresh_tokens.used`, `dotauth.refresh_tokens.invalid`
- authorization code and device flows: `dotauth.authorization_codes.issued`, `dotauth.authorization_codes.redeemed`, `dotauth.authorization_codes.expired`, `dotauth.authorization_codes.invalid`, `dotauth.device_authorization.started`, `dotauth.device_code.polls`, `dotauth.device_code.approval.duration`
- client and user authentication: `dotauth.client.auth.success`, `dotauth.client.auth.failure`, `dotauth.resource_owner.auth.success`, `dotauth.resource_owner.auth.failure`
- UMA, introspection, and user info: `dotauth.uma.rpt.issued`, `dotauth.uma.rpt.request_submitted`, `dotauth.uma.rpt.denied`, `dotauth.uma.ticket.expired`, `dotauth.introspection.requests`, `dotauth.introspection.active`, `dotauth.introspection.inactive`, `dotauth.userinfo.requests`
- operational signals: `dotauth.throttle.allowed`, `dotauth.throttle.rejected`, `dotauth.errors.oauth`, `dotauth.errors.unhandled`, `dotauth.token_store.operation.duration`

### Verifying the telemetry

The acceptance test suite includes OpenTelemetry scenarios that boot a real OpenTelemetry Collector container and assert the exported span and metric names end-to-end.

To run the telemetry acceptance scenarios locally:

```zsh
dotnet test "/Users/jacobreimers/code/dotauth/tests/dotauth.acceptancetests/dotauth.acceptancetests.csproj" --filter FullyQualifiedName~OpenTelemetryInstrumentation --logger "console;verbosity=minimal"
```

## Configuration Values for Demo Servers

The demo servers can be customized by setting the environment variables defined below. In addition to the application specific variables below, the standard ASP.NET environments can also be passed.

Note that some environment variables use double underscore ```__```. This is to ensure compatibility with the .NET conversion from environment variable to hierarchical configuration value.

|Environment Variable|Type|Description|
|---|---|---|
|SALT|string|Defines a hashing salt to be used. Default value is ```string.Empty```.|
|SERVER__NAME|string|Defines a custom name to display as the application name in UI headers. Default value is ```DotAuth```|
|SERVER__REDIRECT|bool|When set to ```true``` then requests for ```/``` or ```/home``` are redirected to ```/authenticate```. This effectively hides the default home page.|
|SERVER__ALLOWSELFSIGNEDCERT|bool|When set to ```true``` then allows self signed certificates and certificates whose root certificate is not trusted. The certificate must still be issued to a valid host.|
|SERVER__ALLOWHTTP|bool|When set to ```true``` then allows downloading OAuth metadata over HTTP. This option should only be set in development environments. Default value is ```false```|
|OAUTH__AUTHORITY|url string|Used to set the OAuth server where authorization for access to management UI.|
|OAUTH__VALIDISSUERS|comma separated url strings|The comma-separated set of valid issuers for access tokens.|
|DB__CONNECTIONSTRING|string|Sets the connection string when using a backing database.|
|DB__REDISCONFIG|string|Sets the connection string for the redis server.|
|AMAZON__ACCESSKEY|string|When set then the server will configure sms authentication.|
|AMAZON__SECRETKEY|string|When set then the server will configure sms authentication.|
|KNOWN_PROXIES|comma separated string|Sets the list of known proxy IP addresses.|

## Reporting Issues and Bugs

When reporting issues and bugs, please provide a clear set of steps to reproduce the issue. The best way is to provide a failing test case as a pull request.

If that is not possible, please provide a set of steps which allow the bug to be reliably reproduced. These steps must also reproduce the issue on a computer that is not your own.

## Contributions

All contributions are appreciated. Please provide them as an issue with an accompanying pull request.

This is an open source project. Work has gone into the [project](https://github.com/thabart/SimpleIdentityServer) it was forked from, as well as the later improvements.
Please respect the license terms and the fact that issues and contributions may not be handled as fast as you may wish. The best way to get your contribution adopted is to make it easy to pull into the code base.
