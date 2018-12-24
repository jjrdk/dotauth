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

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    using Json;
    using Shared;
    using Shared.Requests;

    public class JwsGenerator : IJwsGenerator
    {
        private const string JwsType = "JWT";
        private readonly ICreateJwsSignature _createJwsSignature;

        public JwsGenerator(
            ICreateJwsSignature createJwsSignature)
        {
            _createJwsSignature = createJwsSignature;
        }
        
        public string Generate(
            JwsPayload jwsPayload,
            JwsAlg jwsAlg,
            JsonWebKey jsonWebKey)
        {
            if (jwsPayload == null)
            {
                throw new ArgumentNullException("jwsPayload");
            }

            if (jsonWebKey == null && 
                jwsAlg != JwsAlg.none)
            {
                jwsAlg = JwsAlg.none;
            }

            var protectedHeader = ConstructProtectedHeader(jwsAlg);
            if (jwsAlg != JwsAlg.none)
            {
                protectedHeader.Kid = jsonWebKey.Kid;
            }

            var serializedProtectedHeader = protectedHeader.SerializeWithDataContract();
            var base64EncodedSerializedProtectedHeader = serializedProtectedHeader.Base64Encode();
            var serializedPayload = jwsPayload.SerializeWithJavascript();
            var base64EncodedSerializedPayload = serializedPayload.Base64Encode();
            var combinedProtectedHeaderAndPayLoad =
                $"{base64EncodedSerializedProtectedHeader}.{base64EncodedSerializedPayload}";

            var signedJws = string.Empty;
            if (jwsAlg != JwsAlg.none)
            {
                switch (jsonWebKey.Kty)
                {
                    case KeyType.RSA:
                        signedJws = _createJwsSignature.SignWithRsa(jwsAlg, jsonWebKey.SerializedKey, combinedProtectedHeaderAndPayLoad);
                        break;
                }
            }

            return $"{combinedProtectedHeaderAndPayLoad}.{signedJws}";
        }

        private JwsProtectedHeader ConstructProtectedHeader(JwsAlg alg)
        {
            return new JwsProtectedHeader
            {
                Alg = Enum.GetName(typeof(JwsAlg), alg),
                Type = JwsType
            };
        }
    }
}
