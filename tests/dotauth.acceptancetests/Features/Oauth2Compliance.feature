@oauch
Feature: OAuth2 Compliance
	Scenarios derived from the OAuch test suite (https://oauch.io/Tests).
	OAuch tests an authorization server's compliance with the OAuth 2.0 standard,
	the OAuth threat model (RFC 6819), the Security Best Current Practices,
	and OpenID Connect. Tests are grouped by OAuch category.
	Infrastructure-level tests (TLS version, cipher suites, certificate trust)
	are excluded because they are environment-dependent and verified separately.

Background:
	Given a running auth server
	And the server's signing key

# ---------------------------------------------------------------------------
# Document Support
# Verifies that the discovery document exposes the expected endpoint URIs
# and that the server advertises its capabilities correctly.
# ---------------------------------------------------------------------------

@oauth_DocumentSupport @oauth_OpenIdSupported
Scenario: Discovery document advertises OpenID Connect support
	When requesting the openid configuration document
	Then provider metadata contains all required fields

@oauth_DocumentSupport @oauth_RFC7636Supported
Scenario: Discovery document advertises PKCE support
	When requesting the openid configuration document
	Then provider metadata advertises code_challenge_methods_supported

@oauth_DocumentSupport @oauth_RFC7009Supported
Scenario: Discovery document advertises token revocation endpoint
	When requesting the openid configuration document
	Then provider metadata contains a revocation_endpoint

@oauth_DocumentSupport @oauth_RFC8628Supported
Scenario: Discovery document advertises device authorization grant support
	When requesting discovery document
	Then discovery document has uri for device authorization

@oauth_DocumentSupport @oauth_FormPostSupported
Scenario: Discovery document advertises form_post response mode
	When requesting the openid configuration document
	Then provider metadata includes form_post in response_modes_supported

# ---------------------------------------------------------------------------
# Feature Support
# Verifies the grant types and response types actively supported by the server.
# ---------------------------------------------------------------------------

@oauth_FeatureSupport @oauth_CodeFlowSupported
Scenario: Authorization code grant is supported
	Given a properly configured auth client
	When requesting authorization for scope api1
	Then has authorization uri

@oauth_FeatureSupport @oauth_ClientCredentialsFlowSupported
Scenario: Client credentials grant is supported
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then has valid access token

@oauth_FeatureSupport @oauth_PasswordFlowSupported
Scenario: Password grant is supported
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid
	Then has valid access token for audience client

@oauth_FeatureSupport @oauth_DeviceFlowSupported
Scenario: Device authorization grant is supported
	Given a device token client
	When a device requests authorization
	Then device authorization response contains device_code and user_code

@oauth_FeatureSupport @oauth_HasRefreshTokens
Scenario: Refresh tokens are issued and can be exchanged
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	Then can get new token from refresh token

@oauth_FeatureSupport @oauth_HasAccessTokens
Scenario: Access tokens are returned for client credentials grant
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then has valid access token

@oauth_FeatureSupport @oauth_HasJwtAccessTokens
Scenario: Access tokens are issued as JWTs
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then access token is a valid JWT

@oauth_FeatureSupport @oauth_TokenFlowSupported
Scenario: Implicit flow with response type token is supported
	When requesting implicit flow with response_type token
	Then authorization response contains access_token token_type and expires_in

@oauth_FeatureSupport @oauth_IdTokenFlowSupported
Scenario: Implicit flow with response type id_token is supported
	When requesting implicit flow with response_type id_token
	Then authorization response contains id_token and expected claims

@oauth_FeatureSupport @oauth_IdTokenTokenFlowSupported
Scenario: Implicit flow with response type id_token token is supported
	When requesting implicit flow with response_type id_token token
	Then authorization response contains access_token id_token token_type and expires_in

@oauth_FeatureSupport @oauth_CodeIdTokenFlowSupported
Scenario: Hybrid flow with response type code id_token is supported
	When requesting hybrid flow with response_type code id_token
	Then authorization response contains code and id_token with c_hash

@oauth_FeatureSupport @oauth_CodeTokenFlowSupported
Scenario: Hybrid flow with response type code token is supported
	When requesting hybrid flow with response_type code token
	Then authorization response contains code access_token token_type and expires_in

@oauth_FeatureSupport @oauth_CodeIdTokenTokenFlowSupported
Scenario: Hybrid flow with response type code id_token token is supported
	When requesting hybrid flow with response_type code id_token token
	Then authorization response contains code access_token and id_token with c_hash and at_hash

@oauth_FeatureSupport @oauth_CanSignatureBeVerified
Scenario: Signatures can be verified using JWKS endpoint
	When requesting the jwks endpoint
	Then jwks contains signing keys suitable for id token validation

@oauth_FeatureSupport @oauth_PlainPkce
Scenario: Server advertises plain PKCE support
	When requesting the openid configuration document
	Then provider metadata advertises plain in code_challenge_methods_supported

# ---------------------------------------------------------------------------
# Token Endpoint
# Verifies that the token endpoint enforces security constraints including
# code binding, redirect URI checks, replay prevention, and response headers.
# ---------------------------------------------------------------------------

@oauth_TokenEndpoint @oauth_IsCodeBoundToClient
Scenario: Authorization code is bound to the requesting client
	Given a client credentials token client with client, client
	And a valid authorization code
	When a different client attempts to exchange the authorization code
	Then the token exchange is rejected with invalid_grant or invalid_client

@oauth_TokenEndpoint @oauth_RedirectUriChecked
Scenario: Redirect URI is validated when exchanging authorization code
	Given a properly configured auth client
	When requesting authorization for scope api1
	And exchanging the code using a mismatched redirect URI
	Then the token exchange is rejected with invalid_grant

@oauth_TokenEndpoint @oauth_MultipleCodeExchanges
Scenario: Authorization code cannot be exchanged more than once
	Given a client credentials token client with client, client
	And a valid authorization code
	When the authorization code is exchanged a second time
	Then the second exchange is rejected

@oauth_TokenEndpoint @oauth_TokenValidAfterMultiExchange
Scenario: Access token issued before double code exchange is invalidated
	Given a client credentials token client with client, client
	And a valid authorization code
	When the authorization code is exchanged a first time and then again
	Then the first access token is no longer valid

@oauth_TokenEndpoint @oauth_RefreshTokenValidAfterMultiExchange
Scenario: Refresh tokens are invalidated after double code exchange
	Given a client credentials token client with client, client
	And a valid authorization code
	When the authorization code is exchanged a first time and then again
	Then the refresh token from the first exchange is no longer valid

@oauth_TokenEndpoint @oauth_InvalidatedRefreshToken
Scenario: Active refresh token is revoked after multi-exchange
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	And the refresh token is exchanged twice in rapid succession
	Then only one exchange succeeds and the other is rejected

@oauth_TokenEndpoint @oauth_IsRefreshBoundToClient
Scenario: Refresh token is bound to the client that was issued the original token
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	And a different client attempts to use the refresh token
	Then the token exchange is rejected with invalid_grant or invalid_client

@oauth_TokenEndpoint @oauth_AuthorizationCodeTimeout
Scenario: Authorization codes expire and cannot be exchanged after expiry
	Given a client credentials token client with client, client
	And a valid authorization code that has expired
	When the expired authorization code is exchanged
	Then the token exchange is rejected

@oauth_TokenEndpoint @oauth_RefreshTokenRevokedAfterUse
Scenario: Refresh token rotation invalidates previous refresh token
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	And the refresh token is exchanged for a new token
	And the original refresh token is exchanged again
	Then the second refresh exchange is rejected

@oauth_TokenEndpoint @oauth_UsesTokenRotation
Scenario: Each refresh exchange issues a new refresh token
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	And the refresh token is exchanged for a new token
	Then a new refresh token is issued that differs from the original

@oauth_TokenEndpoint @oauth_IsRefreshAuthenticationRequired
Scenario: Client authentication is required for refresh token grant
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	And an unauthenticated client attempts to exchange the refresh token
	Then the token exchange is rejected

@oauth_TokenEndpoint @oauth_IsClientAuthenticationRequired
Scenario: Client authentication is required at the token endpoint
	When an unauthenticated request is sent to the token endpoint
	Then the server responds with an unauthorized error

@oauth_TokenEndpoint @oauth_IsClientIdRequired
Scenario: Client ID is required at the token endpoint
	When a token request is sent without a client_id
	Then the server responds with an invalid_client or invalid_request error

@oauth_TokenEndpoint @oauth_IsBasicAuthenticationSupported
Scenario: Token endpoint supports HTTP Basic authentication for client credentials
	Given a client credentials token client using basic authentication with clientCredentials, clientCredentials
	When requesting token
	Then has valid access token

@oauth_TokenEndpoint @oauth_IsAuthInUriAllowed
Scenario: Client credentials in the URI are rejected
	When a token request includes client credentials in the query string
	Then the server rejects the request or does not authenticate via URI credentials

@oauth_TokenEndpoint @oauth_HasCacheControlHeader
Scenario: Token endpoint responses include Cache-Control no-store header
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then the response includes Cache-Control header with no-store

@oauth_TokenEndpoint @oauth_HasPragmaHeader
Scenario: Token endpoint responses include Pragma no-cache header
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then the response includes Pragma header with no-cache

@oauth_TokenEndpoint @oauth_IsGetSupported
Scenario: Token endpoint rejects GET requests
	When a GET request is sent to the token endpoint
	Then the server responds with method not allowed or bad request

@oauth_TokenEndpoint @oauth_SameParameterTwiceDisallowed
Scenario: Token endpoint rejects requests with duplicate parameters
	Given a client credentials token client with clientCredentials, clientCredentials
	When a token request is sent with the same parameter duplicated
	Then the server responds with an invalid_request error

@oauth_TokenEndpoint @oauth_UnrecognizedParameterAllowed
Scenario: Token endpoint ignores unrecognized parameters
	Given a client credentials token client with clientCredentials, clientCredentials
	When a token request is sent with an unknown parameter
	Then the server issues a token successfully

@oauth_TokenEndpoint @oauth_RefreshTokenPresent
Scenario: Client credentials grant does not issue refresh tokens
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then the response does not contain a refresh token

@oauth_TokenEndpoint @oauth_IsPasswordFlowDisabled
Scenario: Password grant is disabled or not advertised in discovery
	When requesting the openid configuration document
	Then provider metadata does not list password in grant_types_supported

# ---------------------------------------------------------------------------
# Device Authorization Endpoint
# Verifies the device flow endpoint behaviour.
# ---------------------------------------------------------------------------

@oauth_DeviceAuthEndpoint @oauth_DeviceAuthEndpoint_SameParameterTwiceDisallowed
Scenario: Device authorization endpoint rejects requests with duplicate parameters
	Given a device token client
	When a device authorization request is sent with the same parameter duplicated
	Then the server responds with an invalid_request error

@oauth_DeviceAuthEndpoint @oauth_DeviceAuthEndpoint_UnrecognizedParameterAllowed
Scenario: Device authorization endpoint ignores unrecognized parameters
	Given a device token client
	When a device authorization request is sent with an unknown parameter
	Then the device authorization response is successful

# ---------------------------------------------------------------------------
# Access and Refresh Tokens
# Verifies entropy (randomness) and lifetime constraints on tokens and codes.
# ---------------------------------------------------------------------------

@oauth_Tokens @oauth_AccessTokenEntropyMinReq
Scenario: Access tokens have at least 128 bits of entropy
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then the access token has at least 128 bits of entropy

@oauth_Tokens @oauth_AccessTokenEntropySugReq
Scenario: Access tokens have at least 160 bits of entropy
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then the access token has at least 160 bits of entropy

@oauth_Tokens @oauth_AuthorizationCodeEntropyMinReq
Scenario: Authorization codes have at least 128 bits of entropy
	Given a properly configured auth client
	When requesting authorization for scope api1
	Then the authorization code has at least 128 bits of entropy

@oauth_Tokens @oauth_AuthorizationCodeEntropySugReq
Scenario: Authorization codes have at least 160 bits of entropy
	Given a properly configured auth client
	When requesting authorization for scope api1
	Then the authorization code has at least 160 bits of entropy

@oauth_Tokens @oauth_RefreshTokenEntropyMinReq
Scenario: Refresh tokens have at least 128 bits of entropy
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	Then the refresh token has at least 128 bits of entropy

@oauth_Tokens @oauth_RefreshTokenEntropySugReq
Scenario: Refresh tokens have at least 160 bits of entropy
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	Then the refresh token has at least 160 bits of entropy

@oauth_Tokens @oauth_DeviceCodeEntropy
Scenario: Device codes have at least 128 bits of entropy
	Given a device token client
	When a device requests authorization
	Then the device code has at least 128 bits of entropy

@oauth_Tokens @oauth_TokenTimeout
Scenario: Access tokens have a limited lifetime
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then the access token expiry is set

@oauth_Tokens @oauth_ShortTokenTimeout
Scenario: Access tokens expire within a short period
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then the access token lifetime does not exceed 3600 seconds

# ---------------------------------------------------------------------------
# Identity Tokens
# Verifies that ID tokens contain the required claims and use correct values.
# ---------------------------------------------------------------------------

@oauth_IdTokens @oauth_HasRequiredClaims
Scenario: ID token contains all required OIDC claims
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid
	Then the id token contains required claims iss sub aud exp iat

@oauth_IdTokens @oauth_HasCorrectIssuer
Scenario: ID token issuer matches server identifier
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid
	Then the id token issuer equals the server's issuer URL

@oauth_IdTokens @oauth_HasCorrectAudience
Scenario: ID token audience contains the requesting client ID
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid
	Then the id token audience contains client

@oauth_IdTokens @oauth_HasAuthorizedParty
Scenario: ID token contains azp claim when issued for single audience
	Given a client credentials token client with client, client
	When getting token
	Then the id token contains a valid azp claim

@oauth_IdTokens @oauth_IsSigned
Scenario: ID token is cryptographically signed
	Given a client credentials token client with no_key, no_key
	When getting token
	Then token is signed with server key

@oauth_IdTokens @oauth_HasCorrectMac
Scenario: ID token MAC is correct and verifiable using server JWKS
	Given a client credentials token client with no_key, no_key
	When getting token
	And getting token
	And getting token
	Then token is signed with server key

@oauth_IdTokens @oauth_SigningKeySecure
Scenario: ID token is signed with a secure key
	Given a client credentials token client with no_key, no_key
	When getting token
	Then the signing key has at least 2048 bits for RSA or 256 bits for EC

@oauth_IdTokens @oauth_NoncePresentInToken
Scenario: Nonce supplied in authorization request is present in ID token
	When requesting authorization with nonce
	Then resulting id_token contains matching nonce claim

@oauth_IdTokens @oauth_IsAccessTokenHashPresent
Scenario: at_hash claim is present in ID token for hybrid flows
	When requesting hybrid flow with response_type code id_token token
	Then authorization response contains code access_token and id_token with c_hash and at_hash

@oauth_IdTokens @oauth_IsAccessTokenHashCorrect
Scenario: at_hash claim value correctly reflects the access token
	When requesting hybrid flow with response_type code id_token token
	Then the at_hash claim is the correct left-half SHA-256 hash of the access token

@oauth_IdTokens @oauth_IsAuthorizationCodeHashPresent
Scenario: c_hash claim is present in ID token for hybrid code flows
	When requesting hybrid flow with response_type code id_token
	Then authorization response contains code and id_token with c_hash

@oauth_IdTokens @oauth_CodeHashValid
Scenario: c_hash claim value correctly reflects the authorization code
	When requesting hybrid flow with response_type code id_token
	Then the c_hash claim is the correct left-half SHA-256 hash of the authorization code

@oauth_IdTokens @oauth_KeyReferences
Scenario: Server communicates signing key references via discovery JWKS URI
	When requesting the openid configuration document
	Then provider metadata contains a jwks_uri
	And the jwks_uri resolves to a valid JWKS document with signing keys

@oauth_IdTokens @oauth_ClientSecretLongEnough
Scenario: Client secret is sufficiently long
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid
	Then the client secret used is at least 20 characters long

# ---------------------------------------------------------------------------
# JWTs
# Verifies that the server correctly validates incoming JWT credentials
# and enforces JWT best practices.
# ---------------------------------------------------------------------------

@oauth_Jwt @oauth_AcceptsNoneSignature
Scenario: Server rejects JWT access tokens with none algorithm
	When a resource request is made using a JWT with alg=none
	Then the API server rejects the request with 401 Unauthorized

@oauth_Jwt @oauth_IsSignatureChecked
Scenario: Server rejects JWT access tokens with an invalid signature
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	And the access token signature is tampered with
	Then the API server rejects the tampered token with 401 Unauthorized

@oauth_Jwt @oauth_IsSignatureRequired
Scenario: Server requires a JWT signature and rejects unsigned tokens
	When a resource request is made using an unsigned JWT access token
	Then the API server rejects the request with 401 Unauthorized

@oauth_Jwt @oauth_HasAudienceClaim
Scenario: Server rejects JWT access tokens with wrong audience
	When a resource request is made using a JWT with incorrect audience
	Then the API server rejects the request with 401 Unauthorized

@oauth_Jwt @oauth_HasIssuerClaim
Scenario: Server rejects JWT access tokens with wrong issuer
	When a resource request is made using a JWT with incorrect issuer
	Then the API server rejects the request with 401 Unauthorized

@oauth_Jwt @oauth_HasSubjectClaim
Scenario: Server rejects JWT access tokens missing subject claim
	When a resource request is made using a JWT without subject claim
	Then the API server rejects the request with 401 Unauthorized

@oauth_Jwt @oauth_IsExpirationChecked
Scenario: Server rejects expired JWT access tokens
	When a resource request is made using an expired JWT access token
	Then the API server rejects the request with 401 Unauthorized

@oauth_Jwt @oauth_IsIssuedAtChecked
Scenario: Server rejects JWT access tokens with future iat claim
	When a resource request is made using a JWT with a future issued-at time
	Then the API server rejects the request with 401 Unauthorized

@oauth_Jwt @oauth_IsNotBeforeChecked
Scenario: Server rejects JWT access tokens before their nbf time
	When a resource request is made using a JWT with a future not-before time
	Then the API server rejects the request with 401 Unauthorized

@oauth_Jwt @oauth_IsJwtReplayDetected
Scenario: Server detects and rejects replayed JWT credentials
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	And the same JWT access token is used in a second resource request
	Then the API server detects the replay and rejects the second request

@oauth_Jwt @oauth_SupportsJwtClientAuthentication
Scenario: Server supports JWT-based client authentication
	Given a client with JWT authentication key pair
	When requesting a token using private_key_jwt client authentication
	Then the server issues a valid access token

# ---------------------------------------------------------------------------
# PKCE
# Verifies that PKCE (RFC 7636) is implemented, enforced, and not vulnerable
# to downgrade attacks.
# ---------------------------------------------------------------------------

@oauth_Pkce @oauth_IsPkceImplemented
Scenario: PKCE S256 code challenge is accepted and validated at token exchange
	Given a properly configured auth client
	When requesting authorization for scope api1
	Then has authorization uri

@oauth_Pkce @oauth_IsPkceRequired
Scenario: Authorization code flow requires PKCE for public clients
	When requesting authorization code without required pkce
	Then token exchange is rejected with invalid_grant

@oauth_Pkce @oauth_IsPkceDowngradeDetected
Scenario: PKCE downgrade attack is detected at the authorization request
	When requesting authorization with PKCE then attempting token exchange without code verifier
	Then the token exchange is rejected

@oauth_Pkce @oauth_IsPkceTokenDowngradeDetected
Scenario: PKCE downgrade attack is detected at the token request
	Given a properly configured auth client
	When requesting authorization for scope api1
	And exchanging the code with a mismatched code verifier
	Then token exchange is rejected with invalid_grant

@oauth_Pkce @oauth_HashedPkceDisabled
Scenario: S256 hashed PKCE code challenge is accepted
	Given a properly configured auth client
	When requesting authorization using S256 PKCE
	And exchanging the code with the matching code verifier
	Then has valid access token from token exchange

@oauth_Pkce @oauth_PlainPkceDisabled
Scenario: Plain PKCE code challenge method is disabled or rejected
	Given a properly configured auth client
	When requesting authorization using plain PKCE
	Then the authorization request is rejected or the token exchange fails

@oauth_Pkce @oauth_IsPkcePlainDowngradeDetected
Scenario: Plain PKCE downgrade is detected when server requires S256
	When requesting authorization with S256 PKCE then exchanging with plain verifier
	Then the token exchange is rejected

@oauth_Pkce @oauth_ShortVerifier
Scenario: Code verifiers shorter than 43 characters are rejected
	Given a properly configured auth client
	When requesting authorization using PKCE with an insecure short code verifier
	Then the token exchange is rejected

# ---------------------------------------------------------------------------
# Revocation
# Verifies that the token revocation endpoint (RFC 7009) behaves correctly
# and that revocation cascades appropriately between token types.
# ---------------------------------------------------------------------------

@oauth_Revocation @oauth_CanAccessTokensBeRevoked
Scenario: Access tokens can be revoked via the revocation endpoint
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then can revoke token

@oauth_Revocation @oauth_CanRefreshTokensBeRevoked
Scenario: Refresh tokens can be revoked via the revocation endpoint
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	Then can revoke token

@oauth_Revocation @oauth_RefreshRevokesAccess
Scenario: Revoking the refresh token also invalidates its associated access token
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	And the refresh token is revoked
	Then the associated access token is also invalid

@oauth_Revocation @oauth_AccessRevokesRefresh
Scenario: Revoking the access token also invalidates its associated refresh token
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	And the access token is revoked
	Then the associated refresh token is also invalid

@oauth_Revocation @oauth_IsBoundToClient
Scenario: Token revocation is bound to the client that owns the token
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid
	And a different client attempts to revoke the token
	Then the revocation attempt is rejected or returns an error

@oauth_Revocation @oauth_IsClientAuthRequired
Scenario: Token revocation endpoint requires client authentication
	When an unauthenticated revocation request is sent
	Then the server responds with an unauthorized error

@oauth_Revocation @oauth_IsRevocationEndpointSecure
Scenario: Revocation endpoint is listed in discovery document
	When requesting the openid configuration document
	Then provider metadata contains a revocation_endpoint

# ---------------------------------------------------------------------------
# Concurrency
# Verifies that the server correctly handles rapid or concurrent token exchanges
# to prevent authorization code replay across distributed instances.
# ---------------------------------------------------------------------------

@oauth_Concurrency @oauth_SingleFastACExchange
Scenario: Rapid double exchange of the same authorization code returns error on second attempt
	Given a client credentials token client with client, client
	And a valid authorization code
	When the authorization code is exchanged a second time
	Then the second exchange is rejected

@oauth_Concurrency @oauth_MultiFastACExchange
Scenario: Concurrent double exchange of the same authorization code is handled correctly
	Given a client credentials token client with client, client
	And a valid authorization code
	When the authorization code is exchanged concurrently from two requests
	Then at most one exchange succeeds

@oauth_Concurrency @oauth_SingleFastRefresh
Scenario: Rapid double exchange of the same refresh token invalidates on second attempt
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	And the refresh token is exchanged twice in rapid succession
	Then only one exchange succeeds and the other is rejected

@oauth_Concurrency @oauth_MultiFastRefresh
Scenario: Concurrent double exchange of the same refresh token is handled correctly
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	And the refresh token is exchanged concurrently from two requests
	Then at most one exchange succeeds

@oauth_Concurrency @oauth_ConcurrentTokensRevoked
Scenario: Concurrent token revocations are handled idempotently
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	And the same token is revoked concurrently from two requests
	Then both revocations complete without error

# ---------------------------------------------------------------------------
# Authorization Endpoint
# Verifies the behavior and security of the authorization (front-channel)
# endpoint, including redirect URI validation, state, nonce, and security headers.
# ---------------------------------------------------------------------------

@oauth_AuthorizationEndpoint @oauth_RedirectUriRequired
Scenario: Authorization request requires a redirect_uri for registered clients
	Given a properly configured auth client
	When requesting authorization without a redirect URI
	Then the server responds with an invalid_request error

@oauth_AuthorizationEndpoint @oauth_RedirectUriFullyMatched
Scenario: Authorization endpoint requires exact redirect URI match
	Given a properly configured auth client
	When requesting authorization for wrong callback
	Then has invalid request error message

@oauth_AuthorizationEndpoint @oauth_RedirectUriPathMatched
Scenario: Authorization endpoint rejects redirect URIs that differ only in path
	Given a properly configured auth client
	When requesting authorization with a redirect URI that shares the host but differs in path
	Then has invalid request error message

@oauth_AuthorizationEndpoint @oauth_RedirectUriConfusion
Scenario: Authorization server is not vulnerable to path confusion attacks
	Given a properly configured auth client
	When requesting authorization with a crafted URI designed to exploit path confusion
	Then has invalid request error message

@oauth_AuthorizationEndpoint @oauth_IsResponseTypeChecked
Scenario: Authorization endpoint rejects unsupported response types
	Given a properly configured auth client
	When requesting authorization with an unsupported response type
	Then the server responds with an unsupported_response_type error

@oauth_AuthorizationEndpoint @oauth_InvalidRedirect
Scenario: Authorization endpoint does not auto-redirect for completely invalid redirect URIs
	When an authorization request is made with a completely invalid redirect URI
	Then the server returns an error page rather than redirecting

@oauth_AuthorizationEndpoint @oauth_AutomaticRedirectInvalidScope
Scenario: Authorization endpoint redirects back with error for invalid scope
	Given a properly configured auth client
	When requesting authorization for scope cheese
	Then has invalid scope error message

@oauth_AuthorizationEndpoint @oauth_AutomaticRedirectInvalidResponseType
Scenario: Authorization endpoint redirects back with error for invalid response type
	Given a properly configured auth client
	When requesting authorization with an invalid response type that triggers redirect
	Then the error is returned to the redirect URI rather than displayed as a page

@oauth_AuthorizationEndpoint @oauth_StatePresent
Scenario: State parameter is preserved in the authorization response
	When requesting authorization with state
	Then authorization response contains the original state value

@oauth_AuthorizationEndpoint @oauth_NonceRequired
Scenario: Nonce is required for implicit and hybrid flows
	When requesting implicit flow with response_type id_token without nonce
	Then the authorization request is rejected or the id_token contains no nonce

@oauth_AuthorizationEndpoint @oauth_RequireUserConsent
Scenario: Authorization server requires user consent for new authorizations
	Given a client credentials token client with client, client
	When an authorization request is sent for a new scope
	Then the server presents a consent prompt to the user

@oauth_AuthorizationEndpoint @oauth_SameParameterTwiceDisallowed
Scenario: Authorization endpoint rejects requests with duplicate parameters
	Given a properly configured auth client
	When an authorization request is sent with the same parameter duplicated
	Then the server responds with an invalid_request error

@oauth_AuthorizationEndpoint @oauth_UnrecognizedParameterAllowed
Scenario: Authorization endpoint ignores unrecognized parameters
	Given a properly configured auth client
	When an authorization request is sent with an unknown parameter
	Then the authorization proceeds successfully

@oauth_AuthorizationEndpoint @oauth_HasContentSecurityPolicy
Scenario: Authorization page includes Content-Security-Policy header
	Given a properly configured auth client
	When requesting authorization for scope api1
	And the authorization URI is requested
	Then the response includes a Content-Security-Policy header

@oauth_AuthorizationEndpoint @oauth_HasFrameOptions
Scenario: Authorization page includes X-Frame-Options header
	Given a properly configured auth client
	When requesting authorization for scope api1
	And the authorization URI is requested
	Then the response includes an X-Frame-Options header

@oauth_AuthorizationEndpoint @oauth_ReferrerPolicyEnforced
Scenario: Authorization page suppresses the referrer header
	Given a properly configured auth client
	When requesting authorization for scope api1
	And the authorization URI is requested
	Then the response includes a Referrer-Policy header set to no-referrer

@oauth_AuthorizationEndpoint @oauth_HasFragment
Scenario: Authorization endpoint does not return codes in fragment for code flow
	Given a properly configured auth client
	When requesting authorization for scope api1
	Then the redirect URI does not contain a fragment identifier

@oauth_AuthorizationEndpoint @oauth_FragmentFix
Scenario: Authorization endpoint applies fragment fix for implicit responses
	When requesting implicit flow with response_type token
	Then authorization response contains access_token token_type and expires_in

@oauth_AuthorizationEndpoint @oauth_SupportsPostAuthorizationRequests
Scenario: Authorization endpoint accepts POST requests
	Given a properly configured auth client
	When posting an authorization request to the authorization endpoint
	Then the server handles the POST authorization request

@oauth_AuthorizationEndpoint @oauth_SupportsPostResponseMode
Scenario: Authorization endpoint supports form_post response mode
	When requesting authorization with response_mode form_post
	Then authorization response returns auto-submitting HTML form with response parameters

@oauth_AuthorizationEndpoint @oauth_RefreshTokenPresent_Implicit
Scenario: Implicit flow does not grant refresh tokens
	When requesting implicit flow with response_type token
	Then the authorization response does not contain a refresh token

# ---------------------------------------------------------------------------
# API Endpoint
# Verifies how the resource server (API) validates and processes bearer tokens.
# ---------------------------------------------------------------------------

@oauth_ApiEndpoint @oauth_SupportsAuthorizationHeader
Scenario: API endpoint accepts bearer token in Authorization header
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	Then can get user info

@oauth_ApiEndpoint @oauth_TokenAsQueryParameterDisabled
Scenario: API endpoint rejects token passed as query parameter
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	And calling the userinfo endpoint with token in query string
	Then the userinfo request is rejected or the server ignores the query token

@oauth_ApiEndpoint @oauth_AreBearerTokensDisabled
Scenario: API endpoint requires bearer tokens and rejects requests without one
	When calling the userinfo endpoint without an Authorization header
	Then the server responds with 401 Unauthorized

@oauth_ApiEndpoint @oauth_CacheControl
Scenario: API endpoint sends Cache-Control no-store when token is in URI
	Given a client credentials token client with clientCredentials, clientCredentials
	When requesting token
	And calling the userinfo endpoint with token in query string
	Then the API response includes Cache-Control header with no-store

