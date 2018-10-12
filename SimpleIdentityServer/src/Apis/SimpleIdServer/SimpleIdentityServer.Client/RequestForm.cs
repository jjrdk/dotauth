namespace SimpleIdentityServer.Client
{
    using Core.Common;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class RequestForm : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _form;

        private RequestForm(Dictionary<string, string> form)
        {
            _form = form;
        }

        public static RequestForm FromAuthorizationCode(string code, string redirectUrl, string codeVerifier = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                throw new ArgumentNullException(nameof(redirectUrl));
            }

            var dict = new Dictionary<string, string>
            {
                {RequestTokenNames.Code, code},
                {RequestTokenNames.GrantType, GrantTypes.AuthorizationCode},
                {RequestTokenNames.RedirectUri, redirectUrl}
            };
            if (!string.IsNullOrWhiteSpace(codeVerifier))
            {
                dict.Add(RequestTokenNames.CodeVerifier, codeVerifier);
            }

            return new RequestForm(dict);
        }

        public static RequestForm FromTicketId(string ticketId, string claimToken)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }


            var dict = new Dictionary<string, string>
            {
                {RequestTokenNames.GrantType, GrantTypes.UmaTicket},
                {RequestTokenUma.Ticket, ticketId},
                {RequestTokenUma.ClaimTokenFormat, "http://openid.net/specs/openid-connect-core-1_0.html#IDToken"}
            };


            if (!string.IsNullOrWhiteSpace(claimToken))
            {
                dict.Add(RequestTokenUma.ClaimToken, claimToken);
            }

            return new RequestForm(dict);
        }

        public static RequestForm FromClientCredentials(params string[] scopes)
        {
            if (scopes.Length == 0)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            var dict = new Dictionary<string, string>
            {
                {RequestTokenNames.Scope, string.Join(" ", scopes)},
                {RequestTokenNames.GrantType, GrantTypes.ClientCredentials}
            };

            return new RequestForm(dict);
        }

        public static RequestForm Introspect(string token, TokenType tokenType)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var dict = new Dictionary<string, string>
            {
                { IntrospectionRequestNames.Token, token },
                { IntrospectionRequestNames.TokenTypeHint, tokenType == TokenType.RefreshToken ? TokenTypes.RefreshToken : TokenTypes.AccessToken }
            };

            return new RequestForm(dict);
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

    public class IntrospectForm
    {

    }
}