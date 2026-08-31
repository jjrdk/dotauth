Feature: User Tools
    The list_users and get_user MCP tools expose resource owner information.
    Passwords are never included in any response.

Scenario: get_user returns not found for unknown subject
    Given no user with subject "alice" exists
    When get_user is invoked with subject "alice"
    Then the result indicates the user was not found

Scenario: get_user never returns the password
    Given a user with subject "alice" and password "s3cr3t" exists
    When get_user is invoked with subject "alice"
    Then the result contains "alice"
    And the result does not contain "s3cr3t"

Scenario: list_users indicates listing is not supported when only IResourceOwnerStore is registered
    When list_users is invoked
    Then the result indicates listing is not supported
