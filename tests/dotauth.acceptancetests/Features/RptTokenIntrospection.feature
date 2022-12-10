Feature: Rpt Token Introspection
	Introspection behavior for RPT tokens

Scenario: Can use PAT token to introspect RPT token
	Given a running auth server
	And the server's signing key
	Given a client credentials token client with clientCredentials, clientCredentials
	And a UMA client
	And a PAT token
	And a registered resource
	And an updated authorization policy
	When getting a ticket
	And getting an RPT token
	Then can introspect RPT token using PAT token as authentication