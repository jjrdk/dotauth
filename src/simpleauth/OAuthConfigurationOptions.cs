namespace SimpleAuth
{
    using System;
    using System.Globalization;

    public class OAuthConfigurationOptions
    {
        public OAuthConfigurationOptions(
            TimeSpan authorizationCodeValidity = default,
            CultureInfo defaultLanguage = null,
            params string[] userClaimsToIncludeInAuthToken)
        {
            AuthorizationCodeValidityPeriod = authorizationCodeValidity == default
                ? TimeSpan.FromSeconds(3600)
                : authorizationCodeValidity;
            DefaultLanguage = defaultLanguage ?? CultureInfo.GetCultureInfo("en");
            UserClaimsToIncludeInAuthToken = userClaimsToIncludeInAuthToken ?? Array.Empty<string>();
        }

        public TimeSpan AuthorizationCodeValidityPeriod { get; }
        public CultureInfo DefaultLanguage { get; }
        public string[] UserClaimsToIncludeInAuthToken { get; }
    }
}
