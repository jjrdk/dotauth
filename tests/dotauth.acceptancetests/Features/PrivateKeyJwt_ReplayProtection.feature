@private_key_jwt @finding_G7 @oauth_Jwt
Feature: client_assertion replay protection via jti

	Verifies that a replayed client assertion (same jti within its validity window) is rejected, per RFC 7523 s3 rule #7.
	FINDING G7 (LOW). See private-key-jwt-gap-assessment.md #3 / G7.
	GAPS: there is no client-assertion jti store, so a captured assertion can be replayed until exp.

# -------------------------------------------------------------
# IMPLEMENTATION DESCRIPTION.
# 1. Introduce a client-assertion jti store (an interface plus an in-memory impl for tests and a durable impl for production) keyed by jti with expiry equal to the assertion exp.
# 2. In the client-assertion path, after validation, record the jti; on a second use within the window return invalid_client.
# 3. Make the store pluggable (IClientAssertionStore) via DI so production uses a durable store.
# 4. Honor clock skew and evict expired jti.
# 5. Tests: first assertion accepted; immediate replay rejected; after exp the jti is evicted.

@finding_G7 @replay_rejected
Scenario: A replayed client assertion with the same jti is rejected
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a client assertion signed by that key with a fixed jti
	When requesting a token with client_credentials and client_assertion
	Then the server issues a valid access token
	When requesting the token again with the same client_assertion
	Then the token endpoint responds with error invalid_client

@finding_G7 @distinct_jti_accepted @skip
Scenario: A fresh client assertion with a different jti is accepted
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	When requesting a token with client_credentials and a first client_assertion
	Then the server issues a valid access token
	When requesting a token with client_credentials and a second client_assertion with a new jti
	Then the server issues a valid access token
