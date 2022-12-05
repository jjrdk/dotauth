Feature: Request permission
	Create resource and request permission and access token

@mytag
Scenario: Successful permission creation
	Given a running auth server
	And the server's signing key
	And a properly configured token client
	And a valid UMA token
	And a properly configured uma client
	When registering resource
	And requesting permission
	Then returns ticket id

@mytag
Scenario: Successful permission token grant
	Given a running auth server
	And the server's signing key
	And a properly configured token client
	And a valid UMA token
	And a properly configured uma client
	When registering resource
	And requesting permission
	Then returns ticket id
	And can get access token for resource

Scenario: Successful permissions creation
	Given a running auth server
	And the server's signing key
	And a properly configured token client
	And a valid UMA token
	And a properly configured uma client
	When registering resource
	And requesting permissions
	Then returns ticket id