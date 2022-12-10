Feature: Device Authorization
	Device authorization flow

Scenario: Can get device authorization endpoint from discovery document
	Given a running auth server
	And the server's signing key
	When requesting discovery document
	Then discovery document has uri for device authorization

Scenario: Can authorize device with user approval
	Given a running auth server
	And the server's signing key
	And a token client
	And an access token
	When a device requests authorization
	And the device polls the token server
	And user successfully posts user code
	Then token is returned from polling

Scenario: Can authorize device with user approval when polled too fast
	Given a running auth server
	And the server's signing key
	And a token client
	And an access token
	When a device requests authorization
	And the device polls the token server too fast
	And the device polls the token server polls properly
	And user successfully posts user code
	Then token is returned from polling

Scenario: Polling after expiry gets error
	Given a running auth server
	And the server's signing key
	And a token client
	When a device requests authorization
	And the device polls the token server after expiry
	Then error shows request expiry
