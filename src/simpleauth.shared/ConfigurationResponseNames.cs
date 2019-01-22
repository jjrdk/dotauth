namespace SimpleAuth.Shared
{
    public static class ConfigurationResponseNames
    {
        public const string Issuer = "issuer";
        public const string RegistrationEndpoint = "registration_endpoint";
        public const string TokenEndpoint = "token_endpoint";
        public const string JwksUri = "jwks_uri";
        public const string AuthorizationEndpoint = "authorization_endpoint";
        public const string ClaimsInteractionEndpoint = "claims_interaction_endpoint";
        public const string PoliciesEndpoint = "policies_endpoint";
        public const string IntrospectionEndpoint = "introspection_endpoint";
        public const string ResourceRegistrationEndpoint = "resource_registration_endpoint";
        public const string ClaimTokenProfilesSupported = "claim_token_profiles_supported";
        public const string UmaProfilesSupported = "uma_profiles_supported";
        public const string PermissionEndpoint = "permission_endpoint";
        public const string RevocationEndpoint = "revocation_endpoint";
        public const string ScopesSupported = "scopes_supported";
        public const string ResponseTypesSupported = "response_types_supported";
        public const string GrantTypesSupported = "grant_types_supported";
        public const string TokenEndpointAuthMethodsSupported = "token_endpoint_auth_methods_supported";
        public const string TokenEndpointAuthSigningAlgValuesSupported = "token_endpoint_auth_signing_alg_values_supported";
        public const string UiLocalesSupported = "ui_locales_supported";
    }
}