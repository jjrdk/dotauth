Feature: Dynamic Client Registration
	Dynamic client registration and modification

Background:
	Given a running auth server
	And the server's signing key
	And a client credentials token client with dcr, dcr
	And an out of band dynamic client registration token

Scenario: Can register a new client from HttpClient
	When posting a dynamic client registration request to the auth server
	Then the response should be a 201
	And the response should contain a client_id and client_secret

Scenario: Can register a new client using a DynamicRegistrationClient
	When creating a new DynamicRegistrationClient
	Then can use it to create a new client


Scenario: Can modify a registered client using a DynamicRegistrationClient
	When creating a new DynamicRegistrationClient
	Then can use it to create a new client
	And can modify the registered app
