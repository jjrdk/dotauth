namespace SimpleAuth.Client
{
    using SimpleAuth.Shared.Responses;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class RevokeTokenRequest : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _form;

        private RevokeTokenRequest(Dictionary<string, string> form)
        {
            _form = form;
        }

        public static RevokeTokenRequest RevokeToken(GrantedTokenResponse tokenResponse)
        {
            return RevokeToken(tokenResponse.AccessToken, tokenResponse.TokenType);
        }

        public static RevokeTokenRequest RevokeToken(string token, string tokenType)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var dict = new Dictionary<string, string>
            {
                {"token", token},
                {"token_type_hint", tokenType}
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