@private_key_jwt @finding_G9 @hardening
Feature: private_key_jwt operational hardening and registration surface

	Verifies that client-assertion handling is safe against oversized input, supports multi-value iss / aud, and that the admin UI and tooling can configure private_key_jwt.
	FINDING G9 (LOW). See private-key-jwt-gap-assessment.md #3 / G9.
	GAPS: there is no max-length guard on client_assertion, iss / aud comparison is single-valued, and there is no admin or tool surface for jwks / jwks_uri and token_endpoint_auth_method selection.

# -------------------------------------------------------------
# IMPLEMENTATION DESCRIPTION.
# 1. Add a max-length or segment guard on the client_assertion input.
# 2. Support multi-value aud (a list) and iss where the OP requires it.
# 3. Expose jwks / jwks_uri and token_endpoint_auth_method on the admin UI (dotauth-admin) and dotauth.tool.
# 4. Keep the supported token endpoint auth methods populated in discovery for private_key_jwt and client_secret_jwt.
# 5. Tests: an oversized assertion is rejected, a multi-aud is accepted, and the config UI saves and loads.

@finding_G9 @oversized_rejected
Scenario: An oversized client_assertion is rejected
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a client assertion exceeding the maximum permitted length
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_request

@finding_G9 @multi_aud @skip
Scenario: A client assertion with a multi-value audience is accepted
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a client assertion whose aud is a list including the token endpoint
	When requesting a token with client_credentials and client_assertion
	Then the server issues a valid access token

@finding_G9 @registration_surface
Scenario: A client can be configured for private_key_jwt via the tooling
	Given a running auth server
	And the server's signing key
	And a client management request with token_endpoint_auth_method private_key_jwt and a jwks
	When registering the client via the management API
	Then the client is registered with the private_key_jwt auth method
