Feature: ResourceSet Search
	Specifications for resource set search

	Background: 	
		Given a running auth server
		And the server's signing key
		And a client credentials token client with clientCredentials, clientCredentials
		Given a valid UMA token
		And a properly configured uma client

	Scenario: Find resource set by name
		When registering resources
		  | Name     | Type  | Description     | Scopes      |
		  | picture  | image | some picture    | read,search |
		  | picture2 | image | another picture | read        |
		And searching by term picture
		Then returns 1 search results

	Scenario: Find resource set by type
		When registering resources
		  | Name     | Type  | Description     | Scopes      |
		  | picture  | image | some picture    | read,search |
		And searching by type image and term picture
		Then returns 1 search results

	Scenario: Find resource set by description
		When registering resources
		  | Name    | Type  | Description           | Scopes      |
		  | picture | image | some fancy picture    | read,search |
		  | picture | image | another fancy picture | read,search |
		And searching by term fancy
		Then returns 2 search results

	Scenario: Cannot find unsearchable resources
		When registering resources
		  | Name    | Type  | Description           | Scopes     |
		  | picture | image | some fancy picture    | read,write |
		  | picture | image | another fancy picture | read,write |
		And searching by term fancy
		Then returns 0 search results
