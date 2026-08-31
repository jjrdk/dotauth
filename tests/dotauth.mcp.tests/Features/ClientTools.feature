Feature: Client Tools
    The list_clients and get_client MCP tools expose OAuth client information.
    Secret material is never included in any response.

Scenario: list_clients returns all registered clients without secrets
    Given clients "c1" and "c2" are registered
    When list_clients is invoked
    Then the result contains "c1"
    And the result contains "c2"
    And the result does not contain the secrets field

Scenario: get_client returns not found for unknown client id
    Given no client with id "unknown" is registered
    When get_client is invoked with id "unknown"
    Then the result indicates the client was not found

Scenario: get_client returns client configuration for a known id
    Given a client with id "my-client" is registered
    When get_client is invoked with id "my-client"
    Then the result contains "my-client"
    And the result does not contain the secrets field
