// Copyright © 2017 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Authenticate
{
    using System;
    using System.Linq;
    using Shared.Models;

    internal class ClientTlsAuthentication
    {
        public Client AuthenticateClient(AuthenticateInstruction instruction, Client client)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (instruction.Certificate == null || client.Secrets == null)
            {
                return null;
            }

            var thumbPrint = client.Secrets.FirstOrDefault(s => s.Type == ClientSecretTypes.X509Thumbprint);
            var name = client.Secrets.FirstOrDefault(s => s.Type == ClientSecretTypes.X509Name);
            if (thumbPrint == null || name == null)
            {
                return null;
            }

            var certificate = instruction.Certificate;
            var isSameThumbPrint = thumbPrint.Value == certificate.Thumbprint;
            var isSameName = name.Value == certificate.SubjectName.Name;
            return isSameName && isSameThumbPrint ? client : null;
        }
    }
}
