Feature: Manage Resource Set
	Resource set management requirements

Background: 
	Given a running auth server
	And the server's signing key

Scenario: Can register a resource for a user and manage policies
	Given a client credentials token client with clientCredentials, clientCredentials
	And a UMA client
	When getting a PAT token
	Then can register a resource
	And can view resource policies
	And can update resource policies