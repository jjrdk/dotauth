@private_key_jwt @finding_G8 @oauth_Jwt @regression
Feature: asymmetric coverage for private_key_jwt client authentication

	Verifies that the acceptance suite exercises a true asymmetric key (RS256 / ES256) for private_key_jwt and would fail if the algorithm-confusion defense regresses.
	FINDING G8 (LOW). See private-key-jwt-gap-assessment.md #3 / G8.
	DEFECT: the existing oauth_Jwt scenario in Oauth2Compliance.feature signs with a symmetric HMAC (SymmetricSecurityKey) and hardcodes aud = https://localhost, the exact shape that hides the G3 issue.

# -------------------------------------------------------------
# IMPLEMENTATION DESCRIPTION.
# 1. Replace the HMAC-based private_key_client in the acceptance fixtures (DefaultStores.cs / Oauth2Compliance.cs) with a real asymmetric RS256 or ES256 key pair.
# 2. Keep a happy path that fails if the none / HS-confusion defense (G3) regresses.
# 3. Add key-rotation coverage with multiple kids.
# 4. Use the token endpoint as aud (not a bare scheme plus host).

@finding_G8 @asymmetric_happy_path
Scenario: private_key_jwt with an RS256 key pair authenticates
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a client assertion signed by the private key with aud set to the token endpoint
	When requesting a token with client_credentials and client_assertion
	Then the server issues a valid access token

@finding_G8 @regression_guard
Scenario: The suite fails if the none / HS-confusion defense regresses
	Given a running auth server
	And the server's signing key
	And a client registered as private_key_jwt with an RS256 key pair
	And a client assertion forged as HS256 with the public key as the HMAC secret
	When requesting a token with client_credentials and client_assertion
	Then the token endpoint responds with error invalid_client

