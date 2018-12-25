namespace SimpleAuth
{
    using System;
    using System.Globalization;

    public class OAuthConfigurationOptions
    {
        public OAuthConfigurationOptions(
            TimeSpan tokenValidity = default(TimeSpan),
            TimeSpan authorizationCodeValidity = default(TimeSpan),
            CultureInfo defaultLanguage = null)
        {
            TokenValidityPeriod =
                tokenValidity == default(TimeSpan)
                    ? TimeSpan.FromSeconds(3600)
                    : tokenValidity;
            AuthorizationCodeValidityPeriod = authorizationCodeValidity == default(TimeSpan)
                ? TimeSpan.FromSeconds(3600)
                : authorizationCodeValidity;
            DefaultLanguage = defaultLanguage ?? CultureInfo.GetCultureInfo("en");
        }

        public TimeSpan TokenValidityPeriod { get; }
        public TimeSpan AuthorizationCodeValidityPeriod { get; }
        public CultureInfo DefaultLanguage { get; }
    }
}
