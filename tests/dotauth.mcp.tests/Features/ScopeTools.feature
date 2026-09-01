Feature: Scope Tools
    The list_scopes and get_scope MCP tools expose OAuth 2.0 scope information.

Scenario: list_scopes returns all registered scopes
    Given scopes "openid" and "profile" are registered
    When list_scopes is invoked
    Then the result contains "openid"
    And the result contains "profile"

Scenario: get_scope returns not found for unknown scope
    Given no scope named "unknown" is registered
    When get_scope is invoked with name "unknown"
    Then the result indicates the scope was not found

Scenario: get_scope returns scope details for a known name
    Given a scope named "openid" with description "OpenID scope" is registered
    When get_scope is invoked with name "openid"
    Then the result contains "openid"
    And the result contains "OpenID scope"
