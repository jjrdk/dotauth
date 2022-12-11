Feature: Client Management
	Client management

Background: 
	Given a running auth server
	And a manager client
	And a client credentials token client with manager_client, manager_client
	And a manager token

Scenario: Successful client listing
	When getting all clients
	Then contains list of clients

Scenario: Successful add client
	When adding client
	Then operation succeeds