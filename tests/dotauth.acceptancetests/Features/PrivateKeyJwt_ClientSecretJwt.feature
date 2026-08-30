@private_key_jwt @finding_G4 @oauth_Jwt
Feature: client_secret_jwt assertion handling (JWS, not JWE)

	Verifies that client_secret_jwt uses a JWT signed with the client's shared secret as the MAC key (a 3-segment JWS), correcting the current defect where the path expects a 5-segment JWE.
	FINDING G4 (MEDIUM). See private-key-jwt-gap-assessment.md #3 / G4.
	DEFECT: ClientAssertionAuthentication.AuthenticateClientWithClientSecretJwt requires IsJweToken (5 segments). RFC 7523 defines client_secret_jwt as a JWS keyed by the client secret; it also reads client_id from the request body instead of the assertion.

# -------------------------------------------------------------
# IMPLEMENTATION DESCRIPTION.
# 1. In ClientAssertionAuthentication replace the IsJweToken gate with a JWS check that validates the HS* signature using the client's shared secret.
# 2. Read the client id from the assertion per the profile and reconcile with any body-provided client_id.
# 3. Require client_assertion_type to be client-assertion-type:jwt-bearer and the algorithm to match token_endpoint_auth_signing_alg.
# 4. Keep client_secret_jwt usable only when a SharedSecret exists (the existing guard is correct).
# 5. Tests: a JWS keyed by the secret is accepted; a JWE is not accepted as a secret assertion.

@finding_G4 @jws_accepted
Scenario: A client_secret_jwt signed with the shared secret is accepted
	Given a running auth server
	And the server's signing key
	And a client registered as client_secret_jwt with a shared secret
	And a client assertion (a JWS) signed with that shared secret using HS256
	When requesting a token with client_credentials and client_assertion
	Then the server issues a valid access token

@finding_G4 @wrong_key
Scenario: A client_secret_jwt signed with the wrong secret is rejected
	Given a running auth server
	And the server's signing key
	And a client registered as client_secret_jwt with a shared secret
	And a client assertion (a JWS) signed with a different secret
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_client

