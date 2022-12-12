Feature: Resource owner login flow
	In order to secure access to a resource
	As a resource owner
	I want to log in using resource owner flow

Background: 	
	Given a running auth server
	And the server's signing key
	Given a client credentials token client with client, client

Scenario: Successful authorization
	When getting a token for user, password for scope openid
	Then has valid access token for audience client
	And has valid id token for audience client

Scenario: UserInfo after successful authorization
	When getting a token for user, password for scope openid
	Then has valid access token for audience client
	And can get user info response

Scenario: Successful claims update
	When getting a token for user, password for scope openid,offline
	And has valid access token for audience client
	And updating own claims
	Then update is successful

Scenario: Successful token refresh
	When getting a token for user, password for scope openid,offline
	Then can get new token from refresh token
	And token has custom custom claims

Scenario: Successful token revocation	
	When getting a token for user, password for scope openid
	Then can revoke token

Scenario: Invalid client	
	Given a client credentials token client with xxx, xxx
	When getting a token option for user, password for scope openid
	Then does not have token

Scenario: Invalid user credentials
	When getting a token option for user, xxx for scope openid,offline
	Then does not have token