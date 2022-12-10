Feature: Client Management
	Client management

Scenario: Successful client listing
	Given a running auth server
	And a manager client
	And a token client
	And a manager token
	When getting all clients
	Then contains list of clients

Scenario: Successful add client
	Given a running auth server
	And a manager client
	And a token client
	And a manager token
	When adding client
	Then operation succeeds