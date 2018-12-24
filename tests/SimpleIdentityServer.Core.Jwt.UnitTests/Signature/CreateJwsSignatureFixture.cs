//#region copyright
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
//#endregion

using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Security.Cryptography;
using System.Xml;
using Xunit;

namespace SimpleIdentityServer.Core.Jwt.UnitTests.Signature
{
    using SimpleAuth.Shared;

    public sealed class CreateJwsSignatureFixture
    {
        private ICreateJwsSignature _createJwsSignature;

        [Fact]
        public void When_Trying_To_Rsa_Sign_With_A_Not_Supported_Algorithm_Then_Null_Is_Returned()
        {

            InitializeFakeObjects();

            var result = _createJwsSignature.SignWithRsa(JwsAlg.ES512, string.Empty, string.Empty);

            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_An_Empty_Serialized_Keys_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            Assert.Throws<ArgumentNullException>(() => _createJwsSignature.SignWithRsa(JwsAlg.RS256, string.Empty, string.Empty));
        }

        [Fact]
        public void When_Trying_To_Rsa_Sign_With_Not_Xml_Synthax_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var toBeEncrypted = "toBeEncrypted";
            var serializedKeys = "invalid_serialized_keys";

            Assert.Throws<XmlException>(() => _createJwsSignature.SignWithRsa(JwsAlg.RS256,
                 serializedKeys,
                 toBeEncrypted));
        }

        [Fact]
        public void When_Generate_Rsa_Signature_Then_String_Is_Returned()
        {
            const string messageToBeSigned = "message_to_be_signed";
            InitializeFakeObjects();
            string serializedKeysXml;
            using (var rsa = new RSACryptoServiceProvider())
            {
                serializedKeysXml = RsaExtensions.ToXmlString(rsa, true);
            };
            var signedMessage = _createJwsSignature.SignWithRsa(
                             JwsAlg.RS256,
                             serializedKeysXml,
                             messageToBeSigned);

            Assert.NotNull(signedMessage);
        }

        [Fact]
        public void When_Trying_To_Check_The_Signature_With_A_Not_Supported_Algorithm_Then_False_Is_Returned()
        {

            InitializeFakeObjects();
            var result = _createJwsSignature.VerifyWithRsa(JwsAlg.ES512, string.Empty, string.Empty, new byte[0]);

            Assert.False(result);
        }

        [Fact]
        public void When_Passing_Empty_Serialized_Keys_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            Assert.Throws<ArgumentNullException>(
                () => _createJwsSignature.VerifyWithRsa(JwsAlg.RS256,
                string.Empty,
                string.Empty,
                new byte[0]));
        }

        [Fact]
        public void When_Generate_Correct_Rsa_Signature_And_Checking_It_Then_True_Is_Returned()
        {
            const string messageToBeSigned = "message_to_be_signed";
            InitializeFakeObjects();
            string serializedKeysXml;
            using (var rsa = new RSACryptoServiceProvider())
            {
                serializedKeysXml = RsaExtensions.ToXmlString(rsa, true);
            }

            var signedMessage = _createJwsSignature.SignWithRsa(JwsAlg.RS256,
                serializedKeysXml,
                messageToBeSigned);
            var signature = signedMessage.Base64DecodeBytes();
            var isSignatureCorrect = _createJwsSignature.VerifyWithRsa(JwsAlg.RS256,
                            serializedKeysXml,
                            messageToBeSigned,
                            signature);

            Assert.True(isSignatureCorrect);
        }

        //[Fact]
        //public void When_Passing_EmptyString_To_The_Method_Sign_With_EC_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();

        //    Assert.Throws<ArgumentNullException>(() => _createJwsSignature.SignWithEllipseCurve(null, null));
        //    Assert.Throws<ArgumentNullException>(() => _createJwsSignature.SignWithEllipseCurve("serialized", null));
        //}

        //[Fact]
        //public void When_Passing_Empty_String_To_The_Method_Check_EC_Signature_Then_Exception_Is_Thrown()
        //{
        //    InitializeFakeObjects();

        //    Assert.Throws<ArgumentNullException>(() => _createJwsSignature.VerifyWithEllipticCurve(null, null, null));
        //    Assert.Throws<ArgumentNullException>(() => _createJwsSignature.VerifyWithEllipticCurve("serialized", null, null));
        //    Assert.Throws<ArgumentNullException>(() => _createJwsSignature.VerifyWithEllipticCurve("serialized", "message", null));
        //}

        private void InitializeFakeObjects()
        {
            _createJwsSignature = new CreateJwsSignature();
        }
    }
}
