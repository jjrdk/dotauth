Feature: Correct HTTP status when the Authorization Server is unreachable

  Scenario: UmaServerUnreachableResult returns 503
    Given a UmaServerUnreachableResult instance
    When ExecuteResultAsync is called
    Then the response status code is 503
    And the response contains a Warning header

  Scenario: Response includes a Retry-After hint
    Given a UmaServerUnreachableResult instance
    When ExecuteResultAsync is called
    Then the response contains a Retry-After header with a positive integer value
