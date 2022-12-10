Feature: Sms Login
	Verification of login with SMS

Scenario: Successfully receive token using sms login
	Given a running auth server
	And the server's signing key
	Given a basic authentication token client with client, client
	When requesting an sms
	And then requesting token
	Then has valid access token for audience client
	And has valid id token for audience client