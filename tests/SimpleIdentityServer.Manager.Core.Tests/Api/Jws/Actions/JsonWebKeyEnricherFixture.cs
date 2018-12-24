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

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jws.Actions
{
    using SimpleIdentityServer.Core.Api.Jws.Actions;
    using SimpleIdentityServer.Core.Errors;
    using SimpleIdentityServer.Core.Exceptions;
    using SimpleIdentityServer.Core.Jwt;
    using System;
    using System.Security.Cryptography;
    using SimpleAuth.Shared;
    using Xunit;

    public class JsonWebKeyEnricherFixture
    {
        private IJsonWebKeyEnricher _jsonWebKeyEnricher;

        [Fact]
        public void When_Passing_Null_Parameter_To_GetPublicKeyInformation_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            Assert.Throws<ArgumentNullException>(() => _jsonWebKeyEnricher.GetPublicKeyInformation(null));
        }

        [Fact]
        public void When_Passing_Unsupported_Kty_To_GetPublicKeyInformation_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var jsonWebKey = new JsonWebKey
            {
                Kty = KeyType.oct
            };

            var exception = Assert.Throws<IdentityServerManagerException>(() => _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey));
            Assert.True(exception.Code == ErrorCodes.InvalidParameterCode);
        }

        [Fact]
        public void When_Getting_Rsa_Key_Information_Then_Modulus_And_Exponent_Are_Returned()
        {
            InitializeFakeObjects();
            var serializedRsa = string.Empty;
            using (var rsa = new RSACryptoServiceProvider())
            {
                serializedRsa = RsaExtensions.ToXmlString(rsa, true);
            };

            var jsonWebKey = new JsonWebKey
            {
                Kty = KeyType.RSA,
                SerializedKey = serializedRsa
            };

            var result = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey(JwtConstants.JsonWebKeyParameterNames.RsaKey.ModulusName));
            Assert.True(result.ContainsKey(JwtConstants.JsonWebKeyParameterNames.RsaKey.ExponentName));
        }

        [Fact]
        public void When_Passing_Null_Parameter_To_GetJsonWebKeyInformation_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            Assert.Throws<ArgumentNullException>(() => _jsonWebKeyEnricher.GetJsonWebKeyInformation(null));
        }

        [Fact]
        public void When_Passing_Invalid_Kty_To_GetJsonWebKeyInformation_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var jsonWebKey = new JsonWebKey
            {
                Kty = (KeyType)200
            };

            Assert.Throws<ArgumentException>(() => _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey));
        }

        [Fact]
        public void When_Passing_Invalid_Use_To_GetJsonWebKeyInformation_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var jsonWebKey = new JsonWebKey
            {
                Kty = KeyType.RSA,
                Use = (Use)200
            };

            Assert.Throws<ArgumentException>(() => _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey));
        }

        [Fact]
        public void When_Passing_JsonWebKey_To_GetJsonWebKeyInformation_Then_Information_Are_Returned()
        {
            InitializeFakeObjects();
            var jsonWebKey = new JsonWebKey
            {
                Kty = KeyType.RSA,
                Use = Use.Sig,
                Kid = "kid"
            };

            var result = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey(JwtConstants.JsonWebKeyParameterNames.KeyTypeName));
            Assert.True(result.ContainsKey(JwtConstants.JsonWebKeyParameterNames.UseName));
            Assert.True(result.ContainsKey(JwtConstants.JsonWebKeyParameterNames.AlgorithmName));
            Assert.True(result.ContainsKey(JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName));
        }

        private void InitializeFakeObjects()
        {
            _jsonWebKeyEnricher = new JsonWebKeyEnricher();
        }
    }
}
