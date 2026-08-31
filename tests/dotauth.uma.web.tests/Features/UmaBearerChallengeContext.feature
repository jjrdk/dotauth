Feature: UMA challenge context carries ticket information

  Scenario: Challenge context exposes TicketId and AsUri
    Given a UmaBearerChallengeContext is constructed
    When TicketId is set to "abc123"
    And AsUri is set to "https://as.example.com"
    Then reading TicketId returns "abc123"
    And reading AsUri returns "https://as.example.com"

  Scenario: HandleResponse suppresses the default challenge
    Given a UmaBearerChallengeContext is constructed
    When HandleResponse is called
    Then Handled is true
