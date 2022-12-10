Feature: Client Credentials Login Flow
	Log in using client credentials flow

Background: 
	Given a running auth server
	And the server's signing key
	And a properly configured token client

Scenario: Successful authentication
	When requesting token
	Then has valid access token
	And can get user info

Scenario: Successful token refresh
	When requesting auth token
	Then can get new token from refresh token

Scenario: Successful token revocation
	When requesting token
	Then can revoke token

Scenario: Invalid client
	Given a token client with invalid client credentials
	When attempting to request token
	Then does not have token