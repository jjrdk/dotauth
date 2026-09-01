Feature: Insufficient RPT triggers a new UMA ticket not 403

  Background:
    Given the permission client returns ticket "ticket-001" and AS URI "https://as.example.com/"
    And the resource map resolves "resource-a" to resource set "rs-001"
    And the token client returns a valid protection token

  Scenario: ForbidAsync returns 401 with UMA ticket
    Given a protected endpoint for resource "resource-a" requiring scope "read"
    When ForbidAsync is called on the scheme
    Then the response status is 401
    And the WWW-Authenticate header starts with "UMA"
    And the WWW-Authenticate header contains ticket="ticket-001"
