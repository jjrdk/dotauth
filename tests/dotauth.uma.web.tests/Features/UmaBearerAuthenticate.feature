Feature: RPT permissions claim is available after authentication

  Scenario: RPT with permissions claim - claim is preserved on the principal
    Given a valid RPT JWT containing a permissions claim for resource set "rs-001" scope "read"
    When HandleAuthenticateAsync is called
    Then the result is AuthenticateResult.Success
    And the ClaimsPrincipal has a "permissions" claim
    And CheckResourceAccess for "rs-001" with scope "read" returns true

  Scenario: Standard JWT without permissions claim - authentication succeeds
    Given a valid JWT that contains no permissions claim
    When HandleAuthenticateAsync is called
    Then the result is AuthenticateResult.Success
    And CheckResourceAccess returns false for resource set "rs-any"

  Scenario: Expired RPT causes authentication failure
    Given an RPT JWT with an expiry in the past
    When HandleAuthenticateAsync is called
    Then the result is AuthenticateResult.Fail

  Scenario: RPT without access to the current resource triggers a new ticket
    Given the permission client returns ticket "ticket-001" and AS URI "https://as.example.com/"
    And the resource map resolves "resource-a" to resource set "rs-001"
    And the token client returns a valid protection token
    And a valid JWT that contains no permissions claim
    When HandleAuthenticateAsync is called against a resource-aware endpoint
    Then the response status is 401
    And the WWW-Authenticate header starts with "UMA"
    And the WWW-Authenticate header contains ticket="ticket-001"
