﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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
using System.Linq;

namespace SimpleIdentityServer.Core.Authenticate
{
    using SimpleAuth.Shared.Models;

    public class ClientSecretBasicAuthentication : IClientSecretBasicAuthentication
    {
        public Client AuthenticateClient(AuthenticateInstruction instruction, Client client)
        {
            if (client == null || instruction == null)
            {
                throw new ArgumentNullException("the client or instruction cannot be null");    
            }

            if (client.Secrets == null)
            {
                return null;
            }

            var clientSecret = client.Secrets.FirstOrDefault(s => s.Type == ClientSecretTypes.SharedSecret);
            if (clientSecret == null)
            {
                return null;
            }

            var sameSecret = string.Compare(clientSecret.Value,
                instruction.ClientSecretFromAuthorizationHeader,
                StringComparison.CurrentCultureIgnoreCase) == 0;
            return sameSecret ? client : null;
        }

        public string GetClientId(AuthenticateInstruction instruction)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException("the instruction cannot be null");
            }

            return instruction.ClientIdFromAuthorizationHeader;
        }
    }
}
