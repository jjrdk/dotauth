namespace SimpleAuth
{
    using System;
    using System.Globalization;

    public class OAuthConfigurationOptions
    {
        public OAuthConfigurationOptions(
            TimeSpan authorizationCodeValidity = default(TimeSpan),
            CultureInfo defaultLanguage = null)
        {
            AuthorizationCodeValidityPeriod = authorizationCodeValidity == default(TimeSpan)
                ? TimeSpan.FromSeconds(3600)
                : authorizationCodeValidity;
            DefaultLanguage = defaultLanguage ?? CultureInfo.GetCultureInfo("en");
        }

        public TimeSpan AuthorizationCodeValidityPeriod { get; }
        public CultureInfo DefaultLanguage { get; }
    }
}
