Feature: Unauthorized scope management
	Unauthorized scope management behavior

Background: 
	Given a running auth server
	And a manager client
	And a client credentials token client with admin_client, admin_client
	And an admin token

Scenario: Rejected scope load
	When requesting existing scope
	Then error is returned

Scenario: Rejected add scope
	When adding new scope
	Then error is returned