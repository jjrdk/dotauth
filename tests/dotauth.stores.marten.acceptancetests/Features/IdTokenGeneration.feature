Feature: IdToken Generation
	Id token verification

Background: 
	Given a running auth server
	And the server's signing key

Scenario: Add two numbers
	Given a client credentials token client with no_key, no_key
	When getting token
	And getting token
	And getting token
	Then token has single audience