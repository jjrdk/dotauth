Feature: Local Authentication
	Local authentication behavior verification

Background: 
	Given a running auth server
	And the server's signing key

Scenario: Successful logout
	When logging out
	Then receives redirect to login page

Scenario: Valid local login
	When posting valid local authorization credentials
	Then receives auth cookie

Scenario: Invalid local login
	When posting invalid local authorization credentials
	Then returns login page