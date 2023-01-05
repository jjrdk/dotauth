Feature: Request permission to protected resource
Create resource and request permission and access token

    Background: 	
        Given a running auth server
        And the server's signing key
        And a client credentials token client with clientCredentials, clientCredentials
        Given a valid UMA token
        And a properly configured uma client

    Scenario: Successful permission creation
        When registering resource
          | Name    | Type  | Description | Scopes |
          | picture | image | a picture   | read   |
        And requesting permission for read
        Then returns ticket id

    Scenario: Successful permission token grant
        When registering resource
          | Name    | Type  | Description | Scopes |
          | picture | image | a picture   | read   |
        And requesting permission for read
        And updating policy
        Then returns ticket id
        And can get access token for resource

    Scenario: Successful permissions creation
        When registering resource
          | Name    | Type  | Description | Scopes     |
          | picture | image | a picture   | read.write |
        And requesting permissions
        Then returns ticket id