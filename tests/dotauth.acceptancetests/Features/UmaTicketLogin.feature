Feature: Uma ticket login
	Log in with UMA ticket

Background: 
	Given a running auth server
	And the server's signing key
	Given a client credentials token client with post_client, post_client
	And a UMA client
	When getting a token for user, password for scope uma_protection,offline
	And has valid access token for audience post_client

Scenario: Successful ticket authentication
	When creating resource set
	And getting a redirection
	And getting token from ticket
	Then can get resource with token

Scenario: Instance without policy ticket authentication
	When creating resource set without policy
	And getting a redirection
	Then cannot get token

Scenario: Unsuccessful ticket authentication
	When creating resource set with deviating scopes
	And requesting permission ticket
	Then cannot get token from ticket
