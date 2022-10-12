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

namespace DotAuth.Authenticate;

using System;
using System.Linq;
using DotAuth.Shared.Models;

internal static class ClientSecretBasicAuthentication
{
    public static Client? AuthenticateClient(this AuthenticateInstruction instruction, Client client)
    {
        var clientSecrets = client.Secrets.Where(s => s.Type == ClientSecretTypes.SharedSecret).ToArray();
        if (clientSecrets.Length == 0)
        {
            return null;
        }

        var sameSecret = clientSecrets.Any(
            clientSecret => string.Equals(
                clientSecret.Value,
                instruction.ClientSecretFromAuthorizationHeader,
                StringComparison.CurrentCulture));
        return sameSecret ? client : null;
    }
}