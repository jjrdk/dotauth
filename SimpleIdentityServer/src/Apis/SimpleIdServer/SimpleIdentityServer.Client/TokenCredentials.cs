namespace SimpleIdentityServer.Client
{
    using System.Collections;
    using System.Collections.Generic;
    using Core.Common;

    public class TokenCredentials : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _form;

        private TokenCredentials(Dictionary<string, string> form)
        {
            _form = form;
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