Feature: Device Authorization
	Device authorization flow

Background:
	Given a running auth server
	And the server's signing key
	And a device token client
	
Scenario: Can get device authorization endpoint from discovery document
	When requesting discovery document
	Then discovery document has uri for device authorization

Scenario: Can authorize device with user approval
	Given an access token
	When a device requests authorization
	And the device polls the token server
	And user successfully posts user code
	Then token is returned from polling

Scenario: Can authorize device with user approval when polled too fast
	Given an access token
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
