Feature: Required UMA services are validated at startup

  Scenario: All required services registered - application starts normally
    Given IResourceMap, IUmaPermissionClient, and ITokenClient are registered
    And AddUmaBearer has been called
    When the host is built
    Then no startup exception is thrown

  Scenario: IResourceMap missing causes startup failure
    Given IUmaPermissionClient and ITokenClient are registered but IResourceMap is not
    And AddUmaBearer has been called
    When the host is built
    Then an InvalidOperationException is thrown mentioning "IResourceMap"

  Scenario: ITokenClient missing causes startup failure
    Given IResourceMap and IUmaPermissionClient are registered but ITokenClient is not
    And AddUmaBearer has been called
    When the host is built
    Then an InvalidOperationException is thrown mentioning "ITokenClient"
