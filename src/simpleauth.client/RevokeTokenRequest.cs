namespace SimpleAuth.Client
{
    using SimpleAuth.Shared.Responses;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the revoke token request.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{KeyValuePair}" />
    public class RevokeTokenRequest : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _form;

        private RevokeTokenRequest(Dictionary<string, string> form)
        {
            _form = form;
        }

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="tokenResponse">The token response.</param>
        /// <returns></returns>
        public static RevokeTokenRequest Create(GrantedTokenResponse tokenResponse)
        {
            return Create(tokenResponse.AccessToken, tokenResponse.TokenType);
        }

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="tokenType">Type of the token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">token</exception>
        public static RevokeTokenRequest Create(string token, string tokenType)
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

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _form.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}