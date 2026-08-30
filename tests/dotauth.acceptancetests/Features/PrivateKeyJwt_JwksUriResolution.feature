Feature: per-client jwks_uri resolution for client key material

	Verifies that a client registered with a jwks_uri has its public keys fetched, cached, and used to verify the client assertion, and refreshed when a kid is unknown.
	FINDING G5 (MEDIUM). See private-key-jwt-gap-assessment.md #3 / G5.
	GAPS: Client.cs has jwks_uri commented out and CreateValidationParameters / GetSigningCredentials only read embedded JsonWebKeys. No HTTP fetch, cache, or refresh exists.

# -------------------------------------------------------------
# IMPLEMENTATION DESCRIPTION.
# 1. Add a JwksUri property to src/dotauth.shared/Models/Client.cs.
# 2. Add IJwksStore / IJwksRepository support to fetch a client's JWKS by uri with a TTL cache and refresh-on-kid-miss, respecting cancellation.
# 3. In CreateValidationParameters / GetSigningCredentials use the fetched set when embedded JsonWebKeys is empty and jwks_uri is set.
# 4. Add loopback / private-IP (SSRF) checks on the fetched jwks_uri.
# 5. Tests: happy path via jwks_uri, a cache hit avoids re-fetch, and refresh on a new kid.

Scenario: A client registered with a jwks_uri authenticates via fetched keys
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with a jwks_uri and no embedded jwks
	And that jwks_uri publishes the client's RS256 public key
	And a client assertion signed by the corresponding private key
	When requesting a token with client_credentials and client_assertion
	Then the server issues a valid access token

Scenario: A repeated request reuses the cached key set without re-fetching
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with a jwks_uri
	When requesting a token with client_credentials and client_assertion twice
	Then the jwks_uri is fetched at most once for the cache lifetime

Scenario: A jwks_uri pointing at a loopback or private address is rejected
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with a jwks_uri of http://127.0.0.1/jwks
	And a client assertion signed by any key
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_client
