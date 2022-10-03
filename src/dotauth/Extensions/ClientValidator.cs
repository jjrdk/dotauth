// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Extensions;

using System;
using System.Linq;
using System.Text;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using CodeChallengeMethods = Shared.Models.CodeChallengeMethods;

internal static class ClientValidator
{
    public static Uri[] GetRedirectionUrls(this Client client, params Uri[] urls)
    {
        return client.RedirectionUrls.Length == 0
            ? Array.Empty<Uri>()
            : client.RedirectionUrls.Where(urls.Contains).ToArray();
    }

    public static bool CheckGrantTypes(this Client client, params string[] grantTypes)
    {
        if (client.GrantTypes.Length == 0)
        {
            client.GrantTypes = new[]
            {
                GrantTypes.AuthorizationCode
            };
        }

        return grantTypes.All(gt => client.GrantTypes.Contains(gt));
    }

    public static bool CheckResponseTypes(this Client client, params string[] responseTypes)
    {
        if (client.ResponseTypes.Length == 0)
        {
            client.ResponseTypes = new[]
            {
                ResponseTypeNames.Code
            };
        }

        return responseTypes.All(rt => client.ResponseTypes.Contains(rt));
    }

    public static bool CheckPkce(this Client client, string? codeVerifier, AuthorizationCode code)
    {
        if (!client.RequirePkce)
        {
            return true;
        }

        if (code.CodeChallengeMethod == CodeChallengeMethods.Plain)
        {
            return codeVerifier == code.CodeChallenge;
        }

        if (codeVerifier == null)
        {
            return false;
        }

        var codeChallenge = codeVerifier.ToSha256SimplifiedBase64(Encoding.ASCII);

        return code.CodeChallenge == codeChallenge;
    }
}