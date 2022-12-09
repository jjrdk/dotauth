Feature: Add User
	User creation feature

Scenario: Local account subject not modified during creation
	Given a configured resource owner repository
	And runtime settings
	And an AddUserOperation
	When local account user is added to storage
	Then subject is not modified

Scenario: Local user modification during creation
	Given a configured resource owner repository
	And runtime settings
	And an AddUserOperation
	When local account user is added to storage
	Then user is modified

Scenario: External user modification during creation
	Given a configured resource owner repository
	And runtime settings
	And an AddUserOperation
	When external account user is added to storage
	Then user is modified
