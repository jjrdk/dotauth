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
