@oidc @certification @oidc_conformance
Feature: OIDC Certification
	OIDC certification coverage map for OP conformance scenarios

Background:
	Given a running auth server
	And the server's signing key

@profile_basic @oidc_tc_OP_scope_openid @oidc_tc_OP_response_type_code
Scenario: Basic profile authorization code request succeeds for registered scope
	Given a properly configured auth client
	When requesting authorization for scope api1
	Then has authorization uri

@profile_basic @oidc_tc_OP_invalid_scope
Scenario: Basic profile returns invalid_scope for unauthorized scope
	Given a properly configured auth client
	When requesting authorization for scope cheese
	Then has invalid scope error message

@profile_basic @oidc_tc_OP_invalid_redirect_uri
Scenario: Basic profile returns invalid_request for redirect URI mismatch
	Given a properly configured auth client
	When requesting authorization for wrong callback
	Then has invalid request error message

@profile_basic @oidc_tc_OP_id_token_aud @oidc_tc_OP_id_token_single_aud
Scenario: Id token contains expected audience semantics
	Given a client credentials token client with no_key, no_key
	When getting token
	And getting token
	And getting token
	Then token has single audience

@profile_basic @oidc_tc_OP_id_token_signed @oidc_tc_OP_jwks_verification
Scenario: Id token is signed by server key material
	Given a client credentials token client with no_key, no_key
	When getting token
	Then token is signed with server key

@profile_basic @oidc_tc_OP_userinfo_valid
Scenario: UserInfo succeeds with valid bearer token
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid
	Then can get user info response

@profile_basic @oidc_tc_OP_refresh_token
Scenario: Refresh token flow succeeds for openid offline scope
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid,offline
	Then can get new token from refresh token

@profile_basic @oidc_tc_OP_revocation
Scenario: Token revocation succeeds for issued token
	Given a client credentials token client with client, client
	When getting a token for user, password for scope openid
	Then can revoke token

@profile_basic @oidc_tc_OP_invalid_client
Scenario: Token request fails for invalid client credentials
	Given a client credentials token client with xxx, xxx
	When getting a token option for user, password for scope openid
	Then does not have token

@profile_basic @oidc_tc_OP_invalid_grant
Scenario: Token request fails for invalid resource owner credentials
	Given a client credentials token client with client, client
	When getting a token option for user, xxx for scope openid,offline
	Then does not have token

@profile_registration @oidc_tc_OP_dynamic_client_registration_create
Scenario: Dynamic client registration succeeds over HTTP endpoint
	Given a client credentials token client with dcr, dcr
	And an out of band dynamic client registration token
	When posting a dynamic client registration request to the auth server
	Then the response should be a 201
	And the response should contain a client_id and client_secret

@profile_registration @oidc_tc_OP_dynamic_client_registration_client_library
Scenario: Dynamic registration client can create new registration
	Given a client credentials token client with dcr, dcr
	And an out of band dynamic client registration token
	When creating a new DynamicRegistrationClient
	Then can use it to create a new client

@profile_registration @oidc_tc_OP_dynamic_client_registration_update
Scenario: Dynamic registration client can update existing registration
	Given a client credentials token client with dcr, dcr
	And an out of band dynamic client registration token
	When creating a new DynamicRegistrationClient
	Then can use it to create a new client
	And can modify the registered app

@profile_basic @oidc_tc_OP_authentication_endpoint_code_handling
Scenario: OpenID authentication endpoint accepts protected request code
	Given a data protector instance
	When posting code to openid authentication
	Then response has status code OK

@profile_discovery @oidc_tc_OP_Discovery_Config
Scenario: Discovery document publishes required provider metadata
	Given a running auth server
	When requesting the openid configuration document
	Then provider metadata contains all required fields

@profile_discovery @oidc_tc_OP_Discovery_JWKS
Scenario: Discovery document jwks_uri exposes valid signing keys
	Given a running auth server
	When requesting the jwks endpoint
	Then jwks contains signing keys suitable for id token validation

@profile_discovery @oidc_tc_OP_WebFinger
Scenario: WebFinger discovery resolves issuer for user input identifier
	Given a running auth server
	When requesting webfinger for an account identifier
	Then webfinger response includes issuer relation

@profile_implicit @oidc_tc_OP_implicit_id_token
Scenario: Implicit flow id_token response is returned from authorization endpoint
	Given a running auth server
	When requesting implicit flow with response_type id_token
	Then authorization response contains id_token and expected claims

@profile_implicit @oidc_tc_OP_implicit_id_token_token
Scenario: Implicit flow id_token token response includes access token and id token
	Given a running auth server
	When requesting implicit flow with response_type id_token token
	Then authorization response contains access_token id_token token_type and expires_in

@profile_hybrid @oidc_tc_OP_hybrid_code_id_token
Scenario: Hybrid flow code id_token response is returned with proper hash claims
	Given a running auth server
	When requesting hybrid flow with response_type code id_token
	Then authorization response contains code and id_token with c_hash

@profile_hybrid @oidc_tc_OP_hybrid_code_token
Scenario: Hybrid flow code token response is returned with access token details
	Given a running auth server
	When requesting hybrid flow with response_type code token
	Then authorization response contains code access_token token_type and expires_in

@profile_hybrid @oidc_tc_OP_hybrid_code_id_token_token
Scenario: Hybrid flow code id_token token response includes c_hash and at_hash
	Given a running auth server
	When requesting hybrid flow with response_type code id_token token
	Then authorization response contains code access_token and id_token with c_hash and at_hash

@profile_basic @oidc_tc_OP_nonce
Scenario: Nonce is returned and validated for front-channel responses
	Given a running auth server
	When requesting authorization with nonce
	Then resulting id_token contains matching nonce claim

@profile_basic @oidc_tc_OP_state
Scenario: State is preserved through authorization response
	Given a running auth server
	When requesting authorization with state
	Then authorization response contains the original state value

@profile_basic @oidc_tc_OP_prompt_login
Scenario: Prompt login forces active user authentication
	Given a running auth server
	When requesting authorization with prompt login
	Then the server requires end user authentication before consent

@profile_basic @oidc_tc_OP_response_mode_form_post
Scenario: Form post response mode returns values in HTML form body
	Given a running auth server
	When requesting authorization with response_mode form_post
	Then authorization response returns auto-submitting HTML form with response parameters

@profile_basic @oidc_tc_OP_claims_parameter
Scenario: Claims parameter returns explicitly requested claims
	Given a running auth server
	When requesting authorization with claims parameter
	Then id_token and userinfo include requested claims when available

@profile_logout @oidc_tc_OP_rp_initiated_logout
Scenario: RP initiated logout ends authenticated browser session
	Given a running auth server
	When requesting end session endpoint with valid id_token_hint
	Then session cookie is cleared and post logout redirect is honored

@profile_security @oidc_tc_OP_mixup_mitigation
Scenario: Authorization response includes issuer parameter for mix-up mitigation
	Given a running auth server
	When requesting authorization from multiple issuers context
	Then authorization response includes issuer identifier

@profile_security @oidc_tc_OP_pkce_s256
Scenario: Authorization code flow enforces PKCE S256 for public clients
	Given a running auth server
	When requesting authorization code without required pkce
	Then token exchange is rejected with invalid_grant

