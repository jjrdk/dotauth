Feature: Resource owner management
	Resource owner management behavior verification

Background: 
	Given a running auth server
	And a manager client
	And a client credentials token client with manager_client, manager_client
	And an manager token

Scenario: Success add resource owner
	When adding resource owner with test, test
	Then resource owner test is local account

Scenario: Success update resource owner password
	When adding resource owner with test, test
	Then can update resource owner test with password test2
	And user can login test with new password test2

Scenario: Can update own claims and refresh
	When updating user administrator claims
		| Type | Value |
		| added_claim_test | something |
	Then is ok request
	And has new token
	When refreshing token, then has updated claims

Scenario: Can update own claims and log in again
	When updating user administrator claims
	  | Type | Value |
	  | added_claim_test | something |
	Then is ok request
	And has new token
	When revoking token
	And logging in again
	Then gets updated claim in token
	
Scenario: Can update own claims
	When updating user administrator claims
	  | Type | Value |
	  | added_claim_test | something |
	Then is ok request
	And resource owner has new claim

Scenario: Can delete own claims
	When deleting user claims
	Then is ok request
	And resource owner no longer has claim

Scenario: Cannot delete claims not part of scope
	When deleting user claims not in scope
	Then is ok request
	And when getting resource owner from store
	And resource owner still has claim

Scenario: Can update own claims two times
	When updating user administrator claims
	  | Type | Value |
	  | added_claim_test | something |
	Then has new admin token
	When updating user administrator claims
	  | Type | Value |
	  | added_claim_test2 | something |
	Then is ok request

Scenario: Cannot update other users claims
	When updating user other claims
		| Type | Value |
		| test | something |
	Then is bad request

Scenario: Can delete own account
	When deleting own account
	Then is ok request

Scenario: Administrators can update other users claims
	When putting user claims
	Then is ok request
