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

namespace SimpleAuth.Tests.Api.Jwe
{
    using System;
    using System.Threading.Tasks;
    using Encrypt;
    using Errors;
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Requests;
    using SimpleAuth.Api.Jwe;
    using SimpleAuth.Helpers;
    using SimpleAuth.Signature;
    using Xunit;

    public class JweActionsFixture
    {
        private IJweActions _jweActions;
        private Mock<IJweGenerator> _jweGeneratorStub;
        private Mock<IJsonWebKeyHelper> _jsonWebKeyHelperStub;
        private Mock<IJweParser> _jweParserStub;
        private Mock<IJwsParser> _jwsParserStub;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Are_Thrown()
        {
            InitializeFakeObjects();
            var getJweParameter = new GetJweParameter();
            var getJweParameterWithJwe = new GetJweParameter
            {
                Jwe = "jwe"
            };

            
            Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.GetJweInformation(null)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.GetJweInformation(getJweParameter))
                .ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.GetJweInformation(getJweParameterWithJwe))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Url_Is_Not_Well_Formed_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string url = "not_well_formed";
            var getJweParameter = new GetJweParameter
            {
                Jwe = "jwe",
                Url = url
            };

            
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
                    await _jweActions.GetJweInformation(getJweParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, url));
        }

        [Fact]
        public async Task When_Header_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var getJweParameter = new GetJweParameter
            {
                Jwe = "jwe",
                Url = "http://google.be"
            };
            _jweParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
                    await _jweActions.GetJweInformation(getJweParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheTokenIsNotAValidJwe);
        }

        [Fact]
        public async Task When_JsonWebKey_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var jweProtectedHeader = new JweProtectedHeader
            {
                Kid = "kid"
            };
            var getJweParameter = new GetJweParameter
            {
                Jwe = "jwe",
                Url = "http://google.be/"
            };
            _jweParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jweProtectedHeader);
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(null));

            
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
                    await _jweActions.GetJweInformation(getJweParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound,
                            jweProtectedHeader.Kid,
                            getJweParameter.Url));
        }

        [Fact]
        public async Task When_No_Content_Can_Be_Extracted_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var jweProtectedHeader = new JweProtectedHeader
            {
                Kid = "kid"
            };
            var jsonWebKey = new JsonWebKey();
            var getJweParameter = new GetJweParameter
            {
                Jwe = "jwe",
                Url = "http://google.be/"
            };
            _jweParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jweProtectedHeader);
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(jsonWebKey));
            _jweParserStub.Setup(j => j.Parse(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(string.Empty);

            
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
                    await _jweActions.GetJweInformation(getJweParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheContentCannotBeExtractedFromJweToken);
        }

        [Fact]
        public async Task When_Decrypting_Jwe_With_Symmetric_Key_Then_Result_Is_Returned()
        {
            InitializeFakeObjects();
            const string content = "jws";
            var jweProtectedHeader = new JweProtectedHeader
            {
                Kid = "kid"
            };
            var jwsProtectedHeader = new JwsProtectedHeader();
            var jsonWebKey = new JsonWebKey();
            var getJweParameter = new GetJweParameter
            {
                Jwe = "jwe",
                Url = "http://google.be/",
                Password = "password"
            };
            _jweParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jweProtectedHeader);
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(jsonWebKey));
            _jweParserStub.Setup(j =>
                    j.ParseByUsingSymmetricPassword(It.IsAny<string>(), It.IsAny<JsonWebKey>(), It.IsAny<string>()))
                .Returns(content);
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);

            
            var result = await _jweActions.GetJweInformation(getJweParameter).ConfigureAwait(false);
            Assert.True(result.IsContentJws);
            Assert.True(result.Content == content);
        }

        [Fact]
        public void When_Passing_Null_Parameter_To_GetJweInformation_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.GetJweInformation(null));
        }

        [Fact]
        public void When_Passing_Null_Parameter_To_CreateJwe_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var createJweParameterWithoutUrl = new CreateJweParameter();
            var createJweParameterWithoutJws = new CreateJweParameter
            {
                Url = "url"
            };
            var createJweParameterWithoutKid = new CreateJweParameter
            {
                Url = "url",
                Jws = "jws"
            };

            Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.CreateJwe(null)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.CreateJwe(createJweParameterWithoutUrl))
                .ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.CreateJwe(createJweParameterWithoutJws))
                .ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.CreateJwe(createJweParameterWithoutKid))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Not_Well_Formed_Uri_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string url = "url";
            var createJweParameter = new CreateJweParameter
            {
                Url = url,
                Jws = "jws",
                Kid = "kid"
            };

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
                    await _jweActions.CreateJwe(createJweParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, url));
        }

        [Fact]
        public async Task When_JsonWebKey_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJweParameter = new CreateJweParameter
            {
                Url = url,
                Jws = "jws",
                Kid = kid
            };
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(null));

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(async () =>
                    await _jweActions.CreateJwe(createJweParameter).ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        [Fact]
        public async Task When_Encrypting_Jws_With_Password_Then_Operation_Is_Called()
        {
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJweParameter = new CreateJweParameter
            {
                Url = url,
                Jws = "jws",
                Kid = kid,
                Password = "password"
            };
            var jsonWebKey = new JsonWebKey();
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult(jsonWebKey));

            await _jweActions.CreateJwe(createJweParameter).ConfigureAwait(false);

            _jweGeneratorStub.Verify(j => j.GenerateJweByUsingSymmetricPassword(It.IsAny<string>(),
                It.IsAny<JweAlg>(),
                It.IsAny<JweEnc>(),
                It.IsAny<JsonWebKey>(),
                It.IsAny<string>()));
        }

        [Fact]
        public async Task When_Encrypting_Jws_Then_Operation_Is_Called()
        {
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJweParameter = new CreateJweParameter
            {
                Url = url,
                Jws = "jws",
                Kid = kid,
            };
            var jsonWebKey = new JsonWebKey();
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult(jsonWebKey));

            await _jweActions.CreateJwe(createJweParameter).ConfigureAwait(false);

            _jweGeneratorStub.Verify(j => j.GenerateJwe(It.IsAny<string>(),
                It.IsAny<JweAlg>(),
                It.IsAny<JweEnc>(),
                It.IsAny<JsonWebKey>()));
        }

        private void InitializeFakeObjects()
        {
            _jweGeneratorStub = new Mock<IJweGenerator>();
            _jsonWebKeyHelperStub = new Mock<IJsonWebKeyHelper>();
            _jweParserStub = new Mock<IJweParser>();
            _jwsParserStub = new Mock<IJwsParser>();
            _jweActions = new JweActions(
                _jweGeneratorStub.Object,
                _jsonWebKeyHelperStub.Object,
                _jwsParserStub.Object,
                _jweParserStub.Object);
        }
    }
}
