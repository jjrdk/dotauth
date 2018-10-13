namespace SimpleIdentityServer.Client
{
    using Core.Common;
    using System.Collections;
    using System.Collections.Generic;

    public class TokenCredentials : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _form;

        private TokenCredentials(Dictionary<string, string> form)
        {
            _form = form;
        }

        public static TokenCredentials FromClientId(string clientId)
        {
            var dict = new Dictionary<string, string>
            {
                { ClientAuthNames.ClientId, clientId },
            };

            return new TokenCredentials(dict);
        }

        public static TokenCredentials FromClientCredentials(string clientId, string clientSecret)
        {
            var dict = new Dictionary<string, string>
            {
                { ClientAuthNames.ClientId, clientId },
                { ClientAuthNames.ClientSecret, clientSecret }
            };

            return new TokenCredentials(dict);
        }

        public static TokenCredentials FromClientSecret(string clientAssertion, string clientId)
        {
            var dict = new Dictionary<string, string>
            {
                { ClientAuthNames.ClientId, clientId },
                { ClientAuthNames.ClientAssertion, clientAssertion },
                { ClientAuthNames.ClientAssertionType, ClientAssertionTypes.JwtBearer }
            };

            return new TokenCredentials(dict);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _form.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}