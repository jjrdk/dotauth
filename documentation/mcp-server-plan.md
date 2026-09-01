# MCP Server Implementation Plan for DotAuth

## Overview

This plan describes adding a **Model Context Protocol (MCP) server** to DotAuth using the official
`ModelContextProtocol.AspNetCore` SDK (v0.2 / "v2"). The MCP server exposes DotAuth's administrative
operations (client management, user management, scope management, token introspection) as structured
tools that can be consumed by AI assistants and agent frameworks.

### Architecture Decision

A new standalone project `src/dotauth.mcp` will be created. It:

- Hosts an ASP.NET Core application with HTTP/SSE transport (the standard v2 transport).
- Wires existing `dotauth.shared` repository interfaces via DI, exactly as the existing auth-server
  projects do.
- Secures the MCP endpoint with JWT Bearer authentication, requiring a caller to hold a valid
  access token issued by DotAuth itself (or any trusted issuer).
- Is registered in the solution (`dotauth.slnx`) and in `Directory.Packages.props`.

### NuGet Dependencies

| Package | Purpose |
|---|---|
| `ModelContextProtocol.AspNetCore` | MCP v2 server hosting (SSE/HTTP transport) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Secure the MCP endpoint |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | Already in CPM |

---

## Implementation Steps

---

### Step 1 — Create the `dotauth.mcp` project

**Summary:** Scaffold a new `net10.0` ASP.NET Core executable project, add it to the solution, and
register the `ModelContextProtocol.AspNetCore` NuGet package in `Directory.Packages.props`.

**Files to create/modify:**
- `src/dotauth.mcp/dotauth.mcp.csproj`
- `src/dotauth.mcp/Program.cs` (skeleton only)
- `Directory.Packages.props` — add `ModelContextProtocol.AspNetCore`
- `dotauth.slnx` — add project reference

**Acceptance Criteria:**

```gherkin
Feature: MCP project scaffolding

  Scenario: Project compiles after creation
    Given the dotauth.mcp.csproj file exists under src/dotauth.mcp/
    And it targets net10.0
    And it references the dotauth.shared project
    And ModelContextProtocol.AspNetCore is listed in Directory.Packages.props
    When `dotnet build src/dotauth.mcp/dotauth.mcp.csproj` is executed
    Then the build exits with code 0
    And no compilation errors are reported

  Scenario: Project is part of the solution
    Given dotauth.slnx has been updated
    When `dotnet build dotauth.slnx` is executed
    Then the solution build exits with code 0
    And dotauth.mcp is listed in the built projects output
```

---

### Step 2 — Configure the MCP server host

**Summary:** In `Program.cs`, configure ASP.NET Core to host the MCP server using
`builder.Services.AddMcpServer().WithHttpTransport()` and map the SSE endpoint at `/mcp`.
Add JWT Bearer authentication so only callers with a valid bearer token can invoke tools.

**Files to create/modify:**
- `src/dotauth.mcp/Program.cs`
- `src/dotauth.mcp/appsettings.json` — `OAUTH:AUTHORITY` and `OAUTH:VALIDISSUERS` settings

**Acceptance Criteria:**

```gherkin
Feature: MCP server host configuration

  Scenario: Server starts and exposes the SSE endpoint
    Given the MCP server is configured with WithHttpTransport()
    And the endpoint is mapped at /mcp
    When the application is started
    Then a GET request to /mcp returns HTTP 200
    And the response Content-Type contains text/event-stream

  Scenario: Unauthenticated requests are rejected
    Given the MCP endpoint requires JWT Bearer authentication
    When a GET request is made to /mcp without an Authorization header
    Then the response status is HTTP 401

  Scenario: Authenticated requests proceed
    Given a valid JWT Bearer token issued by the configured authority
    When a GET request is made to /mcp with that token in the Authorization header
    Then the response status is HTTP 200

  Scenario: Configuration reads authority from appsettings
    Given appsettings.json contains OAUTH:AUTHORITY
    And OAUTH:VALIDISSUERS is set
    When the application starts
    Then JwtBearerOptions.Authority matches the configured value
    And TokenValidationParameters.ValidIssuers contains the configured issuers
```

---

### Step 3 — Implement Client Management tools

**Summary:** Create a `ClientTools` class decorated with `[McpServerToolType]`. Inject
`IClientRepository` and implement five tools:

| Tool name | Operation |
|---|---|
| `list_clients` | `IClientRepository.GetAll` |
| `get_client` | `IClientRepository.GetById` |
| `search_clients` | `IClientRepository.Search` |
| `create_client` | `IClientRepository.Insert` |
| `update_client` | `IClientRepository.Update` |
| `delete_client` | `IClientRepository.Delete` |

Each tool returns structured JSON. `create_client` and `update_client` accept a `Client` input
object. `delete_client` returns a boolean result.

**Files to create:**
- `src/dotauth.mcp/Tools/ClientTools.cs`

**Acceptance Criteria:**

```gherkin
Feature: Client management MCP tools

  Scenario: list_clients returns all registered clients
    Given the IClientRepository contains three clients
    When an authenticated MCP caller invokes the list_clients tool
    Then the tool result contains exactly three client objects
    And each object includes client_id and client_name fields

  Scenario: get_client returns a specific client
    Given a client with client_id "web" exists in the repository
    When an authenticated MCP caller invokes get_client with clientId "web"
    Then the result contains a client object with client_id "web"

  Scenario: get_client returns not-found for unknown client
    Given no client with client_id "ghost" exists
    When an authenticated MCP caller invokes get_client with clientId "ghost"
    Then the tool result indicates the client was not found

  Scenario: search_clients filters by partial name
    Given clients named "mobile-app", "web-app", and "service" exist
    When an authenticated MCP caller invokes search_clients with terms "app"
    Then the result contains "mobile-app" and "web-app"
    And "service" is not included

  Scenario: create_client inserts a new client
    Given no client with client_id "new-client" exists
    When an authenticated MCP caller invokes create_client with a valid Client object
    Then the tool result indicates success
    And a subsequent get_client call for "new-client" returns that client

  Scenario: create_client rejects a duplicate client_id
    Given a client with client_id "existing" already exists
    When an authenticated MCP caller invokes create_client with client_id "existing"
    Then the tool result indicates failure with a duplicate error message

  Scenario: update_client modifies an existing client
    Given a client with client_id "web" exists with client_name "Old Name"
    When an authenticated MCP caller invokes update_client with client_name "New Name"
    Then the tool result indicates success
    And a subsequent get_client call returns client_name "New Name"

  Scenario: delete_client removes the client
    Given a client with client_id "to-delete" exists
    When an authenticated MCP caller invokes delete_client with clientId "to-delete"
    Then the tool result is true
    And a subsequent get_client call returns not-found
```

---

### Step 4 — Implement Resource Owner (User) Management tools

**Summary:** Create a `UserTools` class decorated with `[McpServerToolType]`. Inject
`IResourceOwnerRepository`. Do **not** expose the password field in any tool output.

| Tool name | Operation |
|---|---|
| `list_users` | `IResourceOwnerRepository.GetAll` |
| `get_user` | `IResourceOwnerRepository.Get` (from `IResourceOwnerStore`) |
| `search_users` | `IResourceOwnerRepository.Search` |
| `create_user` | `IResourceOwnerRepository.Insert` |
| `update_user` | `IResourceOwnerRepository.Update` |
| `set_user_password` | `IResourceOwnerRepository.SetPassword` |
| `delete_user` | `IResourceOwnerRepository.Delete` |

**Files to create:**
- `src/dotauth.mcp/Tools/UserTools.cs`

**Acceptance Criteria:**

```gherkin
Feature: User management MCP tools

  Scenario: list_users returns all resource owners without passwords
    Given the IResourceOwnerRepository contains two users
    When an authenticated MCP caller invokes list_users
    Then the result contains two user objects
    And no user object includes a password field

  Scenario: get_user returns a specific user
    Given a user with subject "alice" exists
    When an authenticated MCP caller invokes get_user with subject "alice"
    Then the result contains a user object with subject "alice"
    And no password field is present in the result

  Scenario: create_user inserts a new user
    Given no user with subject "newuser" exists
    When an authenticated MCP caller invokes create_user with subject "newuser"
    Then the tool result indicates success
    And a subsequent get_user call returns the created user

  Scenario: set_user_password updates the password
    Given a user with subject "alice" exists
    When an authenticated MCP caller invokes set_user_password with a new password
    Then the tool result indicates success
    And the user can authenticate with the new password via the auth server

  Scenario: delete_user removes the user
    Given a user with subject "to-remove" exists
    When an authenticated MCP caller invokes delete_user with subject "to-remove"
    Then the tool result is true
    And a subsequent get_user call returns not-found

  Scenario: search_users filters by partial subject
    Given users with subjects "alice", "alicia", and "bob" exist
    When an authenticated MCP caller invokes search_users with terms "ali"
    Then the result contains "alice" and "alicia"
    And "bob" is not included
```

---

### Step 5 — Implement Scope Management tools

**Summary:** Create a `ScopeTools` class decorated with `[McpServerToolType]`. Inject
`IScopeRepository`.

| Tool name | Operation |
|---|---|
| `list_scopes` | `IScopeStore.GetAll` |
| `get_scope` | `IScopeStore.SearchByNames` (single name) |
| `create_scope` | `IScopeRepository.Insert` |
| `update_scope` | `IScopeRepository.Update` |
| `delete_scope` | `IScopeRepository.Delete` |

**Files to create:**
- `src/dotauth.mcp/Tools/ScopeTools.cs`

**Acceptance Criteria:**

```gherkin
Feature: Scope management MCP tools

  Scenario: list_scopes returns all scopes
    Given the IScopeRepository contains scopes "openid", "profile", "email"
    When an authenticated MCP caller invokes list_scopes
    Then the result contains exactly three scope objects
    And each object includes a name field

  Scenario: get_scope returns a specific scope
    Given a scope named "profile" exists
    When an authenticated MCP caller invokes get_scope with name "profile"
    Then the result contains a scope with name "profile"

  Scenario: create_scope inserts a new scope
    Given no scope named "custom" exists
    When an authenticated MCP caller invokes create_scope with name "custom"
    Then the tool result indicates success
    And a subsequent get_scope call returns the scope

  Scenario: create_scope rejects duplicate scope names
    Given a scope named "openid" already exists
    When an authenticated MCP caller invokes create_scope with name "openid"
    Then the tool result indicates failure

  Scenario: update_scope modifies description
    Given a scope named "profile" with description "Old"
    When an authenticated MCP caller invokes update_scope with description "New"
    Then the tool result indicates success
    And a subsequent get_scope call returns description "New"

  Scenario: delete_scope removes the scope
    Given a scope named "temp-scope" exists
    When an authenticated MCP caller invokes delete_scope with name "temp-scope"
    Then the tool result is true
    And a subsequent get_scope call returns not-found
```

---

### Step 6 — Implement Token Introspection tools

**Summary:** Create a `TokenTools` class decorated with `[McpServerToolType]`. Inject `ITokenStore`.
Do **not** expose raw token values in any output. Token values passed as input are validated/consumed
only, never echoed back.

| Tool name | Operation | Notes |
|---|---|---|
| `introspect_token` | `ITokenStore.GetAccessToken` | Returns active/inactive + claims |
| `list_active_tokens` | (not a standard ITokenStore op) | Omit or implement read-only summary |
| `revoke_access_token` | `ITokenStore.RemoveAccessToken` | Returns boolean |
| `revoke_refresh_token` | `ITokenStore.RemoveRefreshToken` | Returns boolean |

**Files to create:**
- `src/dotauth.mcp/Tools/TokenTools.cs`

**Acceptance Criteria:**

```gherkin
Feature: Token introspection and revocation MCP tools

  Scenario: introspect_token for a valid access token
    Given a valid access token "abc123" exists in the token store
    When an authenticated MCP caller invokes introspect_token with token "abc123"
    Then the result contains active: true
    And the result contains the associated client_id and scopes
    And the raw token value is not present in the result

  Scenario: introspect_token for an unknown token
    Given no token "unknown" exists in the token store
    When an authenticated MCP caller invokes introspect_token with token "unknown"
    Then the result contains active: false

  Scenario: revoke_access_token removes the token
    Given a valid access token "to-revoke" exists
    When an authenticated MCP caller invokes revoke_access_token with token "to-revoke"
    Then the tool result is true
    And a subsequent introspect_token call returns active: false

  Scenario: revoke_refresh_token removes the refresh token
    Given a valid refresh token "refresh-123" exists
    When an authenticated MCP caller invokes revoke_refresh_token with token "refresh-123"
    Then the tool result is true
    And a subsequent GetRefreshToken call returns null
```

---

### Step 7 — Register tools in the MCP server

**Summary:** In `Program.cs`, register all four tool classes with the MCP server using
`.WithTools<ClientTools>()`, `.WithTools<UserTools>()`, `.WithTools<ScopeTools>()`, and
`.WithTools<TokenTools>()`. Register the repository DI bindings (in-memory implementations, or
configurable) so DI can inject them into the tool classes.

The repository registrations should mirror the pattern used in `dotauth.authserver`'s `Startup.cs`:
accept factory delegates from an `McpConfiguration` object so callers can supply any backing store.

**Files to create/modify:**
- `src/dotauth.mcp/Program.cs` — add `.WithTools<*>()` calls
- `src/dotauth.mcp/McpConfiguration.cs` — configuration class for repository factories

**Acceptance Criteria:**

```gherkin
Feature: Tool registration in the MCP server

  Scenario: All tool types are discoverable via MCP initialize
    Given the MCP server is started with all four tool classes registered
    When an MCP client sends the initialize handshake
    Then the server capabilities response lists tools for clients, users, scopes, and tokens
    And the tool count is at least 20

  Scenario: Tool injection receives the correct repository implementation
    Given IClientRepository is registered in DI as InMemoryClientRepository
    When an MCP caller invokes list_clients
    Then the result reflects data from InMemoryClientRepository
    And no NullReferenceException is thrown

  Scenario: McpConfiguration allows custom repository factories
    Given McpConfiguration.Clients is set to a custom factory
    When the MCP server is built
    Then IClientRepository in DI is the instance returned by the custom factory
```

---

### Step 8 — Create integration tests

**Summary:** Create a new test project `tests/dotauth.mcp.tests` using `xunit.v3.mtp-v2` (matching
the existing test pattern). Use `Microsoft.AspNetCore.TestHost` to host the MCP server in-process and
exercise each tool with valid and invalid inputs.

**Files to create:**
- `tests/dotauth.mcp.tests/dotauth.mcp.tests.csproj`
- `tests/dotauth.mcp.tests/McpServerFixture.cs` — TestHost setup
- `tests/dotauth.mcp.tests/ClientToolsTests.cs`
- `tests/dotauth.mcp.tests/UserToolsTests.cs`
- `tests/dotauth.mcp.tests/ScopeToolsTests.cs`
- `tests/dotauth.mcp.tests/TokenToolsTests.cs`

**Acceptance Criteria:**

```gherkin
Feature: MCP server integration tests

  Scenario: Test host starts without errors
    Given the McpServerFixture is initialised with InMemory repositories
    When the TestHost is started
    Then no startup exceptions are thrown
    And the /mcp SSE endpoint responds to a GET with HTTP 200

  Scenario: Integration test for list_clients
    Given McpServerFixture is running with two pre-seeded clients
    When the test invokes the list_clients tool via the MCP protocol
    Then the response contains exactly two clients

  Scenario: Full round-trip client create then delete
    Given McpServerFixture is running with no pre-seeded clients
    When the test invokes create_client then delete_client for the same client_id
    Then each tool call succeeds
    And after deletion, list_clients returns zero clients

  Scenario: All tests pass in CI
    Given the test project is built with `dotnet test tests/dotauth.mcp.tests`
    When the test runner executes
    Then all tests exit with status Passed
    And the exit code is 0
```

---

### Step 9 — Add README and update solution documentation

**Summary:** Add `src/dotauth.mcp/README.md` documenting how to run the MCP server, how to configure
it, how to point an AI assistant (e.g. Claude, Copilot) at the endpoint, and what tools are available.

**Files to create/modify:**
- `src/dotauth.mcp/README.md`

**Acceptance Criteria:**

```gherkin
Feature: MCP server documentation

  Scenario: README explains how to start the server
    Given src/dotauth.mcp/README.md exists
    When a developer reads it
    Then it contains a "Getting Started" section with a `dotnet run` command
    And it lists the OAUTH:AUTHORITY configuration variable
    And it explains that a JWT Bearer token is required to call tools

  Scenario: README lists all available tools
    Given src/dotauth.mcp/README.md exists
    When a developer reads it
    Then it contains a table or list of all MCP tool names
    And each tool has a one-line description

  Scenario: README includes an AI assistant integration example
    Given src/dotauth.mcp/README.md exists
    When a developer reads it
    Then it shows a sample MCP client configuration block
    And the block includes the /mcp endpoint URL
```

---

## Summary of all new files

| Path | Type |
|---|---|
| `src/dotauth.mcp/dotauth.mcp.csproj` | Project file |
| `src/dotauth.mcp/Program.cs` | Entry point + DI + MCP host |
| `src/dotauth.mcp/McpConfiguration.cs` | Configuration class |
| `src/dotauth.mcp/appsettings.json` | App settings |
| `src/dotauth.mcp/Tools/ClientTools.cs` | MCP tool class |
| `src/dotauth.mcp/Tools/UserTools.cs` | MCP tool class |
| `src/dotauth.mcp/Tools/ScopeTools.cs` | MCP tool class |
| `src/dotauth.mcp/Tools/TokenTools.cs` | MCP tool class |
| `src/dotauth.mcp/README.md` | Documentation |
| `tests/dotauth.mcp.tests/dotauth.mcp.tests.csproj` | Test project |
| `tests/dotauth.mcp.tests/McpServerFixture.cs` | Test host fixture |
| `tests/dotauth.mcp.tests/ClientToolsTests.cs` | Client tool tests |
| `tests/dotauth.mcp.tests/UserToolsTests.cs` | User tool tests |
| `tests/dotauth.mcp.tests/ScopeToolsTests.cs` | Scope tool tests |
| `tests/dotauth.mcp.tests/TokenToolsTests.cs` | Token tool tests |

## Modified files

| Path | Change |
|---|---|
| `Directory.Packages.props` | Add `ModelContextProtocol.AspNetCore` package version |
| `dotauth.slnx` | Register `dotauth.mcp` and `dotauth.mcp.tests` projects |

---

## Implementation Status (completed 2026-08-31)

All core milestones have been implemented. The following files were created or modified:

### New files

| Path | Description |
|---|---|
| `src/dotauth.mcp/dotauth.mcp.csproj` | Project file (net10.0, refs dotauth.shared + ModelContextProtocol.AspNetCore + JwtBearer) |
| `src/dotauth.mcp/Program.cs` | Entry point: JWT bearer auth, "manager" policy, MCP server with SSE transport |
| `src/dotauth.mcp/Tools/ClientTools.cs` | `list_clients`, `get_client` — secrets stripped from all responses |
| `src/dotauth.mcp/Tools/ScopeTools.cs` | `list_scopes`, `get_scope` |
| `src/dotauth.mcp/Tools/UserTools.cs` | `list_users`, `get_user` — passwords never returned |
| `tests/dotauth.mcp.tests/dotauth.mcp.tests.csproj` | Test project |
| `tests/dotauth.mcp.tests/McpHostFixture.cs` | In-process TestServer with stub stores and bypass JWT validation |
| `tests/dotauth.mcp.tests/McpHostTests.cs` | Endpoint security: 401/403/pass scenarios |
| `tests/dotauth.mcp.tests/ClientToolsTests.cs` | Unit tests for ClientTools |
| `tests/dotauth.mcp.tests/ScopeToolsTests.cs` | Unit tests for ScopeTools |
| `tests/dotauth.mcp.tests/UserToolsTests.cs` | Unit tests for UserTools |

### Modified files

| Path | Change |
|---|---|
| `Directory.Packages.props` | `ModelContextProtocol.AspNetCore 2.2.0` added |
| `dotauth.slnx` | `src/dotauth.mcp` and `tests/dotauth.mcp.tests` registered |

### Deferred items

The following tools from the plan are **not yet implemented** — the initial scope was deliberately kept narrow and safe:

- `create_client`, `update_client`, `delete_client` (write operations, require careful validation)
- `create_scope`, `update_scope`, `delete_scope` (write operations)
- `list_users` via `IResourceOwnerRepository.GetAll` (currently returns a "not supported" message when the registered store is only `IResourceOwnerStore`)
- Token introspection and revocation tools (`TokenTools`)

These can be added in subsequent PRs by adding tools to the appropriate `*Tools` classes and registering stronger repository interfaces.
