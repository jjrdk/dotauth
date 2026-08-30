@private_key_jwt @finding_G3 @oauth_Jwt
Feature: client_assertion algorithm binding and none / HS-confusion defense

	Verifies that the token endpoint only accepts the signing algorithm(s) the client and server registered and rejects alg none and algorithm-confusion attacks such as an RSA key reused as an HMAC secret.
	FINDING G3 (HIGH). See private-key-jwt-gap-assessment.md #3 / G3.
	RISK: CreateValidationParameters builds IssuerSigningKeys from the client's own JWKS and does NOT set ValidAlgorithms, so the token's alg is honored and an RS256 public key can be replayed as an HS256 secret, or a none header bypasses signature verification.

# -------------------------------------------------------------
# IMPLEMENTATION DESCRIPTION.
# 1. In ClientExtensions.CreateValidationParameters, set TokenValidationParameters.ValidAlgorithms from the client's TokenEndPointAuthSigningAlg (default RS256) or the OP-supported set.
# 2. Reject alg none explicitly (a valid private_key assertion is always signed).
# 3. For asymmetric auth forbid the symmetric alg family (HS256/384/512) when the registered method is private_key_jwt; allow HS* only for client_secret_jwt.
# 4. Reject an assertion whose alg is not in the allowed set with invalid_client.
# 5. Tests: unit + acceptance for none, RS-as-HS confusion, and unsupported alg.

@finding_G3 @alg_none
Scenario: An unsigned (alg none) client assertion is rejected
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a client assertion with algorithm none and no signature
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_client

@finding_G3 @alg_confusion
Scenario: An RSA public key reused as a symmetric HMAC secret is rejected
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a client assertion signed as HS256 using the RS256 public key as the HMAC secret
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_client

@finding_G3 @alg_unsupported
Scenario: A signing algorithm outside the client's allowed set is rejected
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a client assertion signed with the unsupported algorithm ES384
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_client

@finding_G3 @happy_path
Scenario: A client assertion signed with the client's registered algorithm is accepted
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a client assertion signed with RS256
	When requesting a token with client_credentials and client_assertion
	Then the server issues a valid access token

