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

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jwe.Actions
{
    using Moq;
    using SimpleIdentityServer.Core.Jwt.Encrypt;
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using Xunit;
    using SimpleIdentityServer.Core.Api.Jwe.Actions;
    using SimpleIdentityServer.Core.Errors;
    using SimpleIdentityServer.Core.Exceptions;
    using SimpleIdentityServer.Core.Helpers;
    using SimpleIdentityServer.Core.Parameters;

    public class CreateJweActionFixture
    {
        private Mock<IJweGenerator> _jweGeneratorStub;
        private Mock<IJsonWebKeyHelper> _jsonWebKeyHelperStub;
        private ICreateJweAction _createJweAction;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Are_Thrown()
        {            InitializeFakeObjects();
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

                        Assert.ThrowsAsync<ArgumentNullException>(() => _createJweAction.ExecuteAsync(null)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJweAction.ExecuteAsync(createJweParameterWithoutUrl)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJweAction.ExecuteAsync(createJweParameterWithoutJws)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJweAction.ExecuteAsync(createJweParameterWithoutKid)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Not_Well_Formed_Uri_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            const string url = "url";
            var createJweParameter = new CreateJweParameter
            {
                Url = url,
                Jws = "jws",
                Kid = "kid"
            };

                        var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _createJweAction.ExecuteAsync(createJweParameter).ConfigureAwait(false)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, url));
        }

        [Fact]
        public async Task When_JsonWebKey_Doesnt_Exist_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
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

                        var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _createJweAction.ExecuteAsync(createJweParameter).ConfigureAwait(false)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }
        
        [Fact]
        public async Task When_Encrypting_Jws_With_Password_Then_Operation_Is_Called()
        {            InitializeFakeObjects();
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

                        await _createJweAction.ExecuteAsync(createJweParameter).ConfigureAwait(false);

                        _jweGeneratorStub.Verify(j => j.GenerateJweByUsingSymmetricPassword(It.IsAny<string>(),
                It.IsAny<JweAlg>(),
                It.IsAny<JweEnc>(),
                It.IsAny<JsonWebKey>(),
                It.IsAny<string>()));
        }

        [Fact]
        public async Task When_Encrypting_Jws_Then_Operation_Is_Called()
        {            InitializeFakeObjects();
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

                        await _createJweAction.ExecuteAsync(createJweParameter).ConfigureAwait(false);

                        _jweGeneratorStub.Verify(j => j.GenerateJwe(It.IsAny<string>(),
                It.IsAny<JweAlg>(),
                It.IsAny<JweEnc>(),
                It.IsAny<JsonWebKey>()));
        }

        private void InitializeFakeObjects()
        {
            _jweGeneratorStub = new Mock<IJweGenerator>();
            _jsonWebKeyHelperStub = new Mock<IJsonWebKeyHelper>();
            _createJweAction = new CreateJweAction(
                _jweGeneratorStub.Object,
                _jsonWebKeyHelperStub.Object);
        }
    }
}
