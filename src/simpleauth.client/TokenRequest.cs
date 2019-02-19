namespace SimpleAuth.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Shared;

    public class TokenRequest : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _form;

        private TokenRequest(Dictionary<string, string> form)
        {
            _form = form;
        }

        public static TokenRequest FromAuthorizationCode(string code, string redirectUrl, string codeVerifier = null)
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

            return new TokenRequest(dict);
        }

        public static TokenRequest FromTicketId(string ticketId, string claimToken)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }


            var dict = new Dictionary<string, string>
            {
                {RequestTokenNames.GrantType, GrantTypes.UmaTicket},
                {"ticket", ticketId},
                {"claim_token_format", "http://openid.net/specs/openid-connect-core-1_0.html#IDToken"}
            };


            if (!string.IsNullOrWhiteSpace(claimToken))
            {
                dict.Add("claim_token", claimToken);
            }

            return new TokenRequest(dict);
        }

        public static TokenRequest FromScopes(params string[] scopes)
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

            return new TokenRequest(dict);
        }

        public static TokenRequest FromRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            var dict = new Dictionary<string, string>
            {
                {RequestTokenNames.GrantType, GrantTypes.RefreshToken},
                {RequestTokenNames.RefreshToken, refreshToken}
            };

            return new TokenRequest(dict);
        }

        public static TokenRequest FromPassword(string userName, string password, string[] scopes, params string[] amrValues)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (scopes == null || scopes.Length == 0)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            var dict = new Dictionary<string, string>
            {
                { RequestTokenNames.Username, userName },
                { RequestTokenNames.Password, password },
                { RequestTokenNames.Scope, string.Join(" ", scopes) },
                { RequestTokenNames.GrantType, GrantTypes.Password }
            };
            if (amrValues.Length > 0)
            {
                dict.Add(RequestTokenNames.AmrValues, string.Join(" ", amrValues));
            }

            return new TokenRequest(dict);
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