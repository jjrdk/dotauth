Feature: Local authentication with OpenId
	Verification of OpenId authentication behavior

Background: 
	Given a running auth server
	And the server's signing key

Scenario: Invalid open id code
	Given a data protector instance
	When posting code to openid authentication
	Then response has status code OK