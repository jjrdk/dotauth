Feature: OpenId Connect Authentication
	As a user
	I want to be able to login to the application using OpenId Connect
	So that I can access the application

Scenario: Can complete OpenId Connect authentication
	Given a running authentication server
	When I navigate to the application
	Then I should be redirected to the authentication server
	And I should get openid claims in the returned token
