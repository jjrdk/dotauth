// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Common
{
    using System;
    using System.Collections;

    /// <summary>
    /// Represents a JSON Web Token
    /// </summary>
    [KnownType(typeof(object[]))]
    [KnownType(typeof(string[]))]
    public class JwsPayload : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        public string Issuer => GetStringClaim(StandardClaimNames.Issuer);

        /// <summary>
        /// Gets or sets the audience(s)
        /// </summary>
        public string[] Audiences => GetArrayClaim(StandardClaimNames.Audiences);

        /// <summary>
        /// Gets or sets the expiration time
        /// </summary>
        public double ExpirationTime => GetDoubleClaim(StandardClaimNames.ExpirationTime);

        /// <summary>
        /// Gets or sets the IAT
        /// </summary>
        public double Iat => GetDoubleClaim(StandardClaimNames.Iat);

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public string Jti => GetStringClaim(StandardClaimNames.Jti);

        /// <summary>
        /// Gets or sets the authentication time
        /// </summary>
        public double AuthenticationTime => GetDoubleClaim(StandardClaimNames.AuthenticationTime);

        /// <summary>
        /// Gets or sets the NONCE
        /// </summary>
        public string Nonce => GetStringClaim(StandardClaimNames.Nonce);

        /// <summary>
        /// Gets or sets the authentication context class reference
        /// </summary>
        public string Acr => GetStringClaim(StandardClaimNames.Acr);

        /// <summary>
        /// Gets or sets the Authentication Methods References
        /// </summary>
        [DataMember(Name = "amr")]
        public string Amr => GetStringClaim(StandardClaimNames.Amr);

        /// <summary>
        /// Gets or sets the Authorized party
        /// </summary>
        [DataMember(Name = "azp")]
        public string Azp => GetStringClaim(StandardClaimNames.Azp);

        public string GetStringClaim(string claimName)
        {
            if (!_values.ContainsKey(claimName))
            {
                return null;
            }

            return _values[claimName].ToString();
        }

        public double GetDoubleClaim(string claimName)
        {
            if (!_values.ContainsKey(claimName))
            {
                return default(double);
            }

            var claim = _values[claimName].ToString();
            if (double.TryParse(claim, out double result))
            {
                return result;
            }

            return default(double);
        }

        public string[] GetArrayClaim(string claimName)
        {
            if (!_values.ContainsKey(claimName))
            {
                return new string[0];
            }

            var claim = _values[claimName];

            switch (claim)
            {
                case null:
                    return Array.Empty<string>();
                case string[] strings:
                    return strings;
                case object[] arr:
                    return arr.Select(c => c.ToString()).ToArray();
                case JArray jArr:
                    return jArr.Select(c => c.ToString()).ToArray();
                default:
                    return new[] { claim.ToString() };
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_values).GetEnumerator();
        }

        public void Add(string key, object value)
        {
            if (_values.ContainsKey(key))
            {
                var item = _values[key];
                if (item is object[] arr)
                {
                    _values[key] = arr.Concat(new[] {value}).ToArray();
                }
                else
                {
                    _values[key] = new[] {item, value};
                }
            }
            else
            {
                _values.Add(key, value);
            }
        }

        public void AddRange(IEnumerable<KeyValuePair<string, object>> additionalClaims)
        {
            if (additionalClaims == null)
            {
                return;
            }
            foreach (var additionalClaim in additionalClaims)
            {
                _values.Add(additionalClaim.Key, additionalClaim.Value);
            }
        }

        public void Set(string key, object value)
        {
            _values[key] = value;
        }

        public bool HasClaim(string key)
        {
            return _values.ContainsKey(key);
        }
    }
}
