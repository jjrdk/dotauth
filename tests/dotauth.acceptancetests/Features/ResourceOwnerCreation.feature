Feature: Resource owner creation
	Resource owner creation behavior verification

Background: 
	Given a running auth server
	And a manager client
	And a client credentials token client with manager_client, manager_client
	And an manager token

Scenario: Manager created user
	When manager creates user
	And user logs in
	Then user has custom user claim