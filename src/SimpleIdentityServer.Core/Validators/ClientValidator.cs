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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleIdentityServer.Core.Validators
{
    using Helpers;
    using Shared.Models;

    public class ClientValidator : IClientValidator
    {        
        public IEnumerable<Uri> GetRedirectionUrls(Client client, params Uri[] urls)
        {
            if (urls == null || client?.RedirectionUrls == null || !client.RedirectionUrls.Any())
            {
                return new Uri[0];
            }

            return client.RedirectionUrls.Where(urls.Contains);
        }

        public bool CheckGrantTypes(Client client, params GrantType[] grantTypes)
        {
            if (client == null)
            {
                return false;
            }

            grantTypes = grantTypes ?? Array.Empty<GrantType>();

            if (client.GrantTypes == null || !client.GrantTypes.Any())
            {
                client.GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code
                };
            }

            return grantTypes.All(gt => client.GrantTypes.Contains(gt));
        }
        
        public bool CheckResponseTypes(Client client, params ResponseType[] responseTypes)
        {
            if (client == null)
            {
                return false;
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Any())
            {
                client.ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code
                };
            }

            return client.ResponseTypes != null && responseTypes.All(rt => client.ResponseTypes.Contains(rt));
        }

        public bool CheckPkce(Client client, string codeVerifier, AuthorizationCode code)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (!client.RequirePkce)
            {
                return true;
            }

            if (code.CodeChallengeMethod.Value == CodeChallengeMethods.Plain)
            {
                return codeVerifier == code.CodeChallenge;
            }

            var codeChallenge = codeVerifier.ToSha256SimplifiedBase64(Encoding.ASCII);
            //var hashed = SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            //var codeChallenge = hashed.ToBase64Simplified();
            return code.CodeChallenge == codeChallenge;
        }
    }
}
