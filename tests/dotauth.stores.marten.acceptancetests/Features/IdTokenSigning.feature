Feature: IdToken Signing
	Verification of id token signing behavior

Background: 
	Given a running auth server
	And the server's signing key

Scenario: When client has no signing keys
	Given a client credentials token client with no_key, no_key
	When getting token
	Then token is signed with server key