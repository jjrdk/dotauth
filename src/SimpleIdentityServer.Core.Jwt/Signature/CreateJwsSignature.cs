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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    using Extensions;
    using Shared;

    public class CreateJwsSignature : ICreateJwsSignature
    {
        private readonly IEnumerable<JwsAlg> _supportedAlgs = new List<JwsAlg>
        {
            JwsAlg.RS256,
            JwsAlg.RS384,
            JwsAlg.RS512
        };

        private readonly Dictionary<JwsAlg, string> _mappingWinJwsAlgorithmToRsaHashingAlgorithms = new Dictionary<JwsAlg, string>
        {
            {
                JwsAlg.RS256, "SHA256"
            },
            {
                JwsAlg.RS384, "SHA384"
            },
            {
                JwsAlg.RS512, "SHA512"
            }
        };
        private readonly Dictionary<JwsAlg, HashAlgorithmName> _mappingLinuxJwsAlgorithmToRsaHashingAlgorithms = new Dictionary<JwsAlg, HashAlgorithmName>
        {
            {
                JwsAlg.RS256, HashAlgorithmName.SHA256
            },
            {
                JwsAlg.RS384, HashAlgorithmName.SHA384
            },
            {
                JwsAlg.RS512, HashAlgorithmName.SHA512
            }
        };

        public string SignWithRsa(
            JwsAlg algorithm, 
            string serializedKeys,
            string combinedJwsNotSigned)
        {
            if (!_supportedAlgs.Contains(algorithm))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var hashMethod = _mappingWinJwsAlgorithmToRsaHashingAlgorithms[algorithm];
                    var bytesToBeSigned = Encoding.ASCII.GetBytes(combinedJwsNotSigned);
                    rsa.FromXmlStringNetCore(serializedKeys);
                    var byteToBeConverted = rsa.SignData(bytesToBeSigned, hashMethod);
                    return byteToBeConverted.ToBase64Simplified();
                }
            }
            else
            {
                using (var rsa = new RSAOpenSsl())
                {
                    var hashMethod = _mappingLinuxJwsAlgorithmToRsaHashingAlgorithms[algorithm];
                    var bytesToBeSigned = Encoding.ASCII.GetBytes(combinedJwsNotSigned);
                    rsa.FromXmlStringNetCore(serializedKeys);
                    var byteToBeConverted = rsa.SignData(bytesToBeSigned, 0, bytesToBeSigned.Length, hashMethod, RSASignaturePadding.Pkcs1);
                    return byteToBeConverted.ToBase64Simplified();
                }
            }
        }

        public bool VerifyWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string input,
            byte[] signature)
        {
            if (!_supportedAlgs.Contains(algorithm))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            var plainBytes = Encoding.ASCII.GetBytes(input);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var hashMethod = _mappingWinJwsAlgorithmToRsaHashingAlgorithms[algorithm];
                    rsa.FromXmlStringNetCore(serializedKeys);
                    return rsa.VerifyData(plainBytes, hashMethod, signature);
                }
            }
            else
            {
                using (var rsa = new RSAOpenSsl())
                {
                    var hashMethod = _mappingLinuxJwsAlgorithmToRsaHashingAlgorithms[algorithm];
                    rsa.FromXmlStringNetCore(serializedKeys);
                    return rsa.VerifyData(plainBytes, signature, hashMethod, RSASignaturePadding.Pkcs1);
                }

            }
        }
    }
}
