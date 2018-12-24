namespace SimpleIdentityServer.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using SimpleAuth.Shared;

    public class RevokeTokenRequest : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _form;

        private RevokeTokenRequest(Dictionary<string, string> form)
        {
            _form = form;
        }

        public static RevokeTokenRequest RevokeToken(string token, string tokenType)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var dict = new Dictionary<string, string>
            {
                {IntrospectionRequestNames.Token, token},
                {IntrospectionRequestNames.TokenTypeHint, tokenType}
            };
            return new RevokeTokenRequest(dict);
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