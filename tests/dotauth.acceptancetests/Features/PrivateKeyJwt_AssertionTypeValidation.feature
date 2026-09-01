@private_key_jwt @finding_G1 @oauth_Jwt
Feature: client_assertion_type validation for private_key_jwt

	Verifies that the token endpoint only accepts a JWT client assertion when client_assertion_type equals the RFC 7523 value urn:ietf:params:oauth:client-assertion-type:jwt-bearer, and otherwise rejects the request with invalid_client.
	FINDING G1 (HIGH). See private-key-jwt-gap-assessment.md #3 / G1.
	SPEC: RFC 7523 s2.2, s3.2 - unknown assertion types MUST be rejected.

# -------------------------------------------------------------
# IMPLEMENTATION DESCRIPTION.
# 1. In ClientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt, compare instruction.ClientAssertionType against 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer' and return a null client plus error when it does not match or is missing.
# 2. In AuthenticateClient.Authenticate, the PrivateKeyJwt case must NOT fall through to a non-JWT auth method when an assertion is present but no matching type; return invalid_client.
# 3. client_assertion_type mapping already exists (TokenRequest / MappingExtensions To*GrantTypeParameter); keep it and surface the value in the error_detail on mismatch.
# 4. Error code MUST be invalid_client with HTTP 400.
# 5. Tests: negative unit + acceptance cases; unit target tests/dotauth.tests/Authenticate/ClientAssertionAuthenticationFixture.cs.

@finding_G1 @assertion_type_mismatch
Scenario: A wrong client_assertion_type is rejected with invalid_client
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a valid client assertion signed with that key pair
	When requesting a token with client_credentials and client_assertion
	And client_assertion_type is the unsupported value urn:ietf:params:oauth:client-assertion-type:unsupported
	Then the token endpoint responds with error invalid_client
	And no access token is issued

@finding_G1 @missing_assertion_type
Scenario: A missing client_assertion_type does not fall back to client_secret
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a valid client assertion signed with that key pair
	When requesting a token with client_credentials and client_assertion but no client_assertion_type
	Then the token endpoint responds with error invalid_client

@finding_G1 @happy_path
Scenario: The correct client_assertion_type is accepted
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a valid client assertion signed with that key pair
	When requesting a token with client_credentials and client_assertion
	And client_assertion_type is urn:ietf:params:oauth:client-assertion-type:jwt-bearer
	Then the server issues a valid access token

