Feature: Authorized Scope Management
	Handle scope management for authorized users

Scenario: Successful scope management
	Given a running auth server
	And a manager client
	And a token client
	And a manager token
	When requesting existing scope
	Then scope information is returned