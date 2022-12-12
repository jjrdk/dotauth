Feature: Unauthorized resource owner management
	Unauthorized resource management behavior

Background: 
	Given a running auth server
	And a manager client
	And a client credentials token client with admin_client, admin_client
	And an admin token

Scenario: Reject add resource owner
	When adding resource owner
	Then add response has error
	
Scenario: Reject update resource owner password
	When updating resource owner password
	Then update response has error
	
Scenario: RejectUpdateResourceOwnerClaims
	When updating resource owner password
	Then update response has error
	
Scenario: Reject delete resource owner
	When deleting resource owner
	Then delete response has error

Scenario: Rejected list resource owners
	When listing resource owners
	Then list response has error