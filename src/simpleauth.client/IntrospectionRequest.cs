namespace SimpleAuth.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the introspection request.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{KeyValuePair}" />
    public class IntrospectionRequest : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _form;

        private IntrospectionRequest(Dictionary<string, string> form)
        {
            _form = form;
        }

        /// <summary>
        /// Creates the specified request.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="tokenType">Type of the token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">token</exception>
        public static IntrospectionRequest Create(string token, string tokenType)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var dict = new Dictionary<string, string>
            {
                { "token", token },
                { "token_type_hint", tokenType }
            };

            return new IntrospectionRequest(dict);
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