Feature: PAT token user info
	User info retrieval using PAT token

Background:
	Given a running auth server
	And the server's signing key
	And a client credentials token client with clientCredentials, clientCredentials

Scenario: Can get user info for PAT token
	When getting a PAT token for administrator, password
	Then can get user information