Feature: MCP Endpoint Security
    The /mcp endpoint is protected by JWT bearer authentication.
    Only callers holding a valid token with the 'manager' scope are allowed.

Scenario: Anonymous request is rejected
    Given no authorization token
    When a POST request is sent to the MCP endpoint
    Then the response status is 401

Scenario: Request without manager scope is rejected
    Given a bearer token with scope "openid profile"
    When a POST request is sent to the MCP endpoint
    Then the response status is 403

Scenario: Request with manager scope is accepted
    Given a bearer token with scope "openid manager"
    When a POST request is sent to the MCP endpoint
    Then the response status is not 401 or 403
