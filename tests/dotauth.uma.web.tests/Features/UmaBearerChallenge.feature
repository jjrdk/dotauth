Feature: UMA-compliant challenge response

  Background:
    Given the permission client returns ticket "ticket-001" and AS URI "https://as.example.com/"
    And the resource map resolves "resource-a" to resource set "rs-001"
    And the token client returns a valid protection token

  Scenario: No token triggers 401 with UMA ticket
    Given a protected endpoint for resource "resource-a" requiring scope "read"
    When a request is made without an Authorization header
    Then the response status is 401
    And the WWW-Authenticate header starts with "UMA"
    And the WWW-Authenticate header contains as_uri="https://as.example.com/"
    And the WWW-Authenticate header contains ticket="ticket-001"

  Scenario: Realm is included when configured
    Given a protected endpoint for resource "resource-a" requiring scope "read"
    And the scheme options have Realm set to "myapi"
    When a request is made without an Authorization header
    Then the WWW-Authenticate header contains realm="myapi"

  Scenario: Protection token failure results in 503
    Given a protected endpoint for resource "resource-a" requiring scope "read"
    And the token client cannot return a protection token
    When a request is made without an Authorization header
    Then the response status is 503

  Scenario: Permission endpoint failure results in 503
    Given a protected endpoint for resource "resource-a" requiring scope "read"
    And the permission client throws an exception
    When a request is made without an Authorization header
    Then the response status is 503
