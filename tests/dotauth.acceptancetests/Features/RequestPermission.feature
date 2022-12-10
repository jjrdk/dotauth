Feature: Request permission to protected resource
	Create resource and request permission and access token

Background: 	
	Given a running auth server
	And the server's signing key
	And a client credentials token client with clientCredentials, clientCredentials
	Given a valid UMA token
	And a properly configured uma client

@RequestPermission
Scenario: Successful permission creation
	When registering resource
	And requesting permission
	Then returns ticket id

@RequestPermission
Scenario: Successful permission token grant
	When registering resource
	And requesting permission
	And updating policy
	Then returns ticket id
	And can get access token for resource

@RequestPermission
Scenario: Successful permissions creation
	When registering resource
	And requesting permissions
	Then returns ticket id