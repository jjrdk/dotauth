Feature: UMA bearer options configuration

  Scenario: Default values are applied when options are not explicitly set
    Given a new instance of UmaBearerOptions
    Then IdTokenHeader equals "id_token"
    And ResourceIdParameters is empty
    And ResourceSetIdFormat is null
    And Realm is null

  Scenario: Options accept UMA-specific configuration
    Given a UmaBearerOptions instance
    When IdTokenHeader is set to "x-id-token"
    And ResourceIdParameters is set to "tenantId,resourceId"
    And ResourceSetIdFormat is set to "{0}:{1}"
    And Realm is set to "api"
    Then IdTokenHeader equals "x-id-token"
    And ResourceIdParameters contains "tenantId" and "resourceId"
    And ResourceSetIdFormat equals "{0}:{1}"
    And Realm equals "api"
