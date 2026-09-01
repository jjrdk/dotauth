@private_key_jwt @finding_G6 @oauth_Jwt
Feature: registration validation of key material for private_key_jwt

	Verifies that a client cannot be registered or updated as private_key_jwt or client_secret_jwt without valid matching key material, and that empty key sets do not fall back to the server's default signing key at authentication time.
	FINDING G6 (MEDIUM). See private-key-jwt-gap-assessment.md #3 / G6.
	GAPS: ClientFactory.Build has no private_key_jwt branch and GetSigningCredentials can fall through to the AS default key or FirstOrDefault on an empty set.

# -------------------------------------------------------------
# IMPLEMENTATION DESCRIPTION.
# 1. In ClientFactory.Build add a branch: when TokenEndPointAuthMethod is private_key_jwt require at least one of jwks or jwks_uri, validate the key type, the presence of a kid, and that algs are in the OP-supported set.
# 2. For client_secret_jwt require a SharedSecret.
# 3. In GetSigningCredentials prevent falling back to the AS default key for CLIENT AUTHENTICATION (the default key is only for OP-issued tokens).
# 4. Return invalid_client or invalid_request at registration on bad metadata.
# 5. Tests: registering private_key_jwt without keys is rejected; with valid keys is accepted.

@finding_G6 @register_no_keys
Scenario: Registering a private_key_jwt client without key material is rejected
	Given a running auth server
	And the server's signing key
	And a client registration request with token_endpoint_auth_method private_key_jwt
	And no jwks and no jwks_uri
	When registering the client
	Then the registration is rejected

@finding_G6 @register_with_keys
Scenario: Registering a private_key_jwt client with a valid key is accepted
	Given a running auth server
	And the server's signing key
	And a client registration request with token_endpoint_auth_method private_key_jwt
	And a jwks containing an RS256 public key
	When registering the client
	Then the client is registered

@finding_G6 @no_default_fallback
Scenario: A client with an empty key set does not authenticate using the server default key
	Given a running auth server
	And the server's signing key
	And a private_key_jwt client whose stored key set is empty
	And a client assertion signed by any key
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_client

