Feature: UmaFilterAttribute delegates UMA challenge to the authentication handler

  Background:
    Given the permission client returns ticket "ticket-001" and AS URI "https://as.example.com/"
    And the resource map resolves "resource-a" to resource set "rs-001"
    And the token client returns a valid protection token

  Scenario: Unauthenticated user triggers handler challenge
    Given a filter for resource parameter "resource" requiring scope "read"
    And an unauthenticated HTTP context with route value "resource" = "resource-a"
    When the filter runs authorization
    Then ChallengeAsync is called with the UMA scheme

  Scenario: Authenticated user with sufficient permissions passes
    Given a filter for resource parameter "resource" requiring scope "read"
    And an authenticated user with permissions for resource set "rs-001" scope "read"
    And the HTTP context route value "resource" = "resource-a"
    When the filter runs authorization
    Then the filter result is null

  Scenario: Authenticated user with insufficient permissions triggers challenge
    Given a filter for resource parameter "resource" requiring scope "write"
    And an authenticated user with permissions for resource set "rs-001" scope "read"
    And the HTTP context route value "resource" = "resource-a"
    When the filter runs authorization
    Then ChallengeAsync is called with the UMA scheme

  Scenario: Allowed OAuth scope short-circuits the UMA check
    Given a filter for resource parameter "resource" requiring scope "read" with allowed scope "admin"
    And a user with OAuth scope "admin"
    And the HTTP context route value "resource" = "resource-a"
    When the filter runs authorization
    Then the filter result is null
