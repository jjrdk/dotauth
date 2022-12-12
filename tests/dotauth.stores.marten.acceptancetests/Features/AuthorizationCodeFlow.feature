Feature: Authorization Code Flow
	Generate authorization code

Background:
	Given a running auth server
	And the server's signing key
	And a properly configured auth client
	
Scenario: Successful authorization code grant
	When requesting authorization for scope api1
	Then has authorization uri

Scenario: Scope does not match client registration
	When requesting authorization for scope cheese
	Then has invalid scope error message

Scenario: Redirect uri does not match client registration
	When requesting authorization for wrong callback
	Then has invalid request error message