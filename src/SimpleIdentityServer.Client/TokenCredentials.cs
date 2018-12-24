﻿namespace SimpleIdentityServer.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using SimpleAuth.Shared;

    public class TokenCredentials : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _form;

        private TokenCredentials(Dictionary<string, string> form, string authorizationValue = null, X509Certificate2 certificate = null)
        {
            _form = form;
            AuthorizationValue = authorizationValue;
            Certificate = certificate;
        }

        public string AuthorizationValue { get; }

        public X509Certificate2 Certificate { get; }

        public static TokenCredentials FromCertificate(string clientId, X509Certificate2 certificate)
        {
            var dict = new Dictionary<string, string>
            {
                { ClientAuthNames.ClientId, clientId },
            };

            return new TokenCredentials(dict, null, certificate);
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

        public static TokenCredentials FromBasicAuthentication(string clientId, string clientSecret)
        {
            var dict = new Dictionary<string, string>
            {
                { ClientAuthNames.ClientId, clientId },
                { ClientAuthNames.ClientSecret, clientSecret }
            };

            return new TokenCredentials(dict, Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret)));
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