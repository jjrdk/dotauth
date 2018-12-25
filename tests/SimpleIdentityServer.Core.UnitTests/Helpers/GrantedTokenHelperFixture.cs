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

using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    using SimpleAuth;
    using SimpleAuth.Helpers;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Validators;

    public class GrantedTokenHelperFixture
    {
        private Mock<ITokenStore> _grantedTokenRepositoryStub;
        private Mock<IGrantedTokenValidator> _grantedTokenValidatorStub;
        private IGrantedTokenHelper _grantedTokenHelper;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _grantedTokenHelper.GetValidGrantedTokenAsync(null, null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _grantedTokenHelper.GetValidGrantedTokenAsync("scopes", null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Valid_Token_Doesnt_Exist_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JwsPayload>(), It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult((GrantedToken)null));

                        var result = await _grantedTokenHelper.GetValidGrantedTokenAsync("scopes", "client_id", null, null).ConfigureAwait(false);

                        Assert.Null(result);
        }

        [Fact]
        public async Task When_GrantedToken_Is_Expired_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JwsPayload>(), It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult(new GrantedToken()));

            _grantedTokenValidatorStub.Setup(g => g.CheckGrantedToken(It.IsAny<GrantedToken>()))
                .Returns(new GrantedTokenValidationResult
                {
                    IsValid = false
                });

                        var result = await _grantedTokenHelper.GetValidGrantedTokenAsync("scopes", "client_id", null, null).ConfigureAwait(false);

                        Assert.Null(result);
        }

        [Fact]
        public async Task When_Token_Exists_Then_GrantedToken_Is_Returned()
        {            InitializeFakeObjects();
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JwsPayload>(), It.IsAny<JwsPayload>()))
                .Returns(Task.FromResult(new GrantedToken()));
            _grantedTokenValidatorStub.Setup(g => g.CheckGrantedToken(It.IsAny<GrantedToken>()))
                .Returns(new GrantedTokenValidationResult
                {
                    IsValid = true
                });

                        var result = await _grantedTokenHelper.GetValidGrantedTokenAsync("scopes", "client_id", null, null).ConfigureAwait(false);

                        Assert.NotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _grantedTokenRepositoryStub = new Mock<ITokenStore>();
            _grantedTokenValidatorStub = new Mock<IGrantedTokenValidator>();
            _grantedTokenHelper = new GrantedTokenHelper(
                _grantedTokenRepositoryStub.Object,
                _grantedTokenValidatorStub.Object);
        }
    }
}
