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

using System;
using System.Linq;

namespace SimpleIdentityServer.Core.Extensions
{
    using Shared;
    using Shared.Models;

    public static class ClientExtensions
    {
        public static JwsAlg? GetIdTokenSignedResponseAlg(this Client client)
        {
            var algName = client.IdTokenSignedResponseAlg;
            return GetDefaultJwsAlg(algName);
        }

        public static JweAlg? GetIdTokenEncryptedResponseAlg(this Client client)
        {
            var algName = client.IdTokenEncryptedResponseAlg;
            return GetDefaultEncryptAlg(algName);
        }

        public static JweEnc? GetIdTokenEncryptedResponseEnc(this Client client)
        {
            var encName = client.IdTokenEncryptedResponseEnc;
            return GetDefaultEncryptEnc(encName);
        }

        public static JwsAlg? GetUserInfoSignedResponseAlg(this Client client)
        {
            var algName = client.UserInfoSignedResponseAlg;
            return GetDefaultJwsAlg(algName);
        }

        public static JweAlg? GetUserInfoEncryptedResponseAlg(this Client client)
        {
            var algName = client.UserInfoEncryptedResponseAlg;
            return GetDefaultEncryptAlg(algName);
        }

        public static JweEnc? GetUserInfoEncryptedResponseEnc(this Client client)
        {
            var encName = client.UserInfoEncryptedResponseEnc;
            return GetDefaultEncryptEnc(encName);
        }

        public static JwsAlg? GetRequestObjectSigningAlg(this Client client)
        {
            var algName = client.RequestObjectSigningAlg;
            return GetDefaultJwsAlg(algName);
        }
        
        public static JweAlg? GetRequestObjectEncryptionAlg(this Client client)
        {
            var algName = client.RequestObjectEncryptionAlg;
            return GetDefaultEncryptAlg(algName);
        }

        public static JweEnc? GetRequestObjectEncryptionEnc(this Client client)
        {
            var encName = client.RequestObjectEncryptionEnc;
            return GetDefaultEncryptEnc(encName);
        }

        private static JweAlg? GetDefaultEncryptAlg(string algName)
        {
            JweAlg? algEnum = null;
            if (!string.IsNullOrWhiteSpace(algName) &&
                Jwt.JwtConstants.MappingNameToJweAlgEnum.Keys.Contains(algName))
            {
                algEnum = Jwt.JwtConstants.MappingNameToJweAlgEnum[algName];
            }

            return algEnum;
        }

        private static JweEnc? GetDefaultEncryptEnc(string encName)
        {
            JweEnc? encEnum = null;
            if (!string.IsNullOrWhiteSpace(encName) &&
                Jwt.JwtConstants.MappingNameToJweEncEnum.Keys.Contains(encName))
            {
                encEnum = Jwt.JwtConstants.MappingNameToJweEncEnum[encName];
            }

            return encEnum;
        }

        private static JwsAlg? GetDefaultJwsAlg(string algName)
        {
            JwsAlg? signedAlgorithm = null;
            JwsAlg result;
            if (!string.IsNullOrWhiteSpace(algName)
                && Enum.TryParse(algName, true, out result))
            {
                signedAlgorithm = result;
            }

            return signedAlgorithm;
        }
    }
}
