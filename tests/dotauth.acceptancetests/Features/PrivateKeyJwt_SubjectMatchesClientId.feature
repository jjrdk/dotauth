@private_key_jwt @finding_G2 @oauth_Jwt
Feature: client_assertion subject equals client_id

	Verifies that the subject (sub) claim of a private_key_jwt client assertion MUST equal the authenticated client's client_id, per RFC 7523 s3 rule #2, and that the issuer (iss) is the registered client identifier.
	FINDING G2 (HIGH). See private-key-jwt-gap-assessment.md #3 / G2.
	SPEC: RFC 7523 s3 rule #2 - for client authentication, sub MUST be the client_id. The lookup currently uses iss and sub is never compared.

# -------------------------------------------------------------
# IMPLEMENTATION DESCRIPTION.
# 1. In the client-assertion path, after resolving the client, require jwtPayload.Subject == resolvedClientId and document the iss == client_id registration convention.
# 2. Reject when sub != client_id with invalid_client.
# 3. Reject when iss was not registered as this client (prevents 'iss of one client == client_id of another').
# 4. Location: ClientAssertionAuthentication (GetClientId returns iss as the lookup id); add the sub == clientId check in AuthenticateClientWithPrivateKeyJwt / AuthenticateClient.
# 5. Add negative acceptance + unit tests for sub != client_id.

@finding_G2 @subject_mismatch
Scenario: A client assertion whose subject differs from client_id is rejected
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with client_id client_a
	And a client assertion signed by that client but with subject other_id
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_client

@finding_G2 @issuer_mismatch
Scenario: A client assertion whose issuer is not the registered identifier is rejected
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with client_id client_a
	And a client assertion with issuer unknown_issuer signed by the client key
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_client

@finding_G2 @happy_path
Scenario: A client assertion whose subject equals client_id is accepted
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with client_id client_a
	And a client assertion with subject client_a and issuer client_a
	When requesting a token with client_credentials and client_assertion
	Then the server issues a valid access token

