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
    using SimpleAuth.Errors;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Helpers;
    using SimpleAuth.JwtToken;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    public class GrantedTokenGeneratorHelperFixture
    {
        private OAuthConfigurationOptions _simpleIdentityServerConfiguratorStub;
        private Mock<IJwtGenerator> _jwtGeneratorStub;
        private Mock<IClientHelper> _clientHelperStub;
        private Mock<IClientStore> _clientRepositoryStub;
        private IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        
        [Fact]
        public async Task When_Passing_NullOrWhiteSpace_Then_Exceptions_Are_Thrown()
        {            InitializeFakeObjects();

            
            await Assert.ThrowsAsync<ArgumentNullException>(() => _grantedTokenGeneratorHelper.GenerateTokenAsync(string.Empty, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>())).Returns(Task.FromResult((Client)null));

            
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _grantedTokenGeneratorHelper.GenerateTokenAsync("invalid_client", null, null, null)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidClient);
            Assert.True(ex.Message == ErrorDescriptions.TheClientIdDoesntExist);
        }

        [Fact]
        public async Task When_ExpirationTime_Is_Set_Then_ExpiresInProperty_Is_Set()
        {
            var client = new Client
            {
                ClientId = "client_id"
            };            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

                        var result = await _grantedTokenGeneratorHelper.GenerateTokenAsync("client_id", "scope", "issuer", null).ConfigureAwait(false);

                        Assert.NotNull(result);
            Assert.True(result.ExpiresIn == 3700);
        }

        private void InitializeFakeObjects()
        {
            _simpleIdentityServerConfiguratorStub = new OAuthConfigurationOptions(tokenValidity:TimeSpan.FromSeconds(3700));
            _jwtGeneratorStub = new Mock<IJwtGenerator>();
            _clientHelperStub = new Mock<IClientHelper>();
            _clientRepositoryStub = new Mock<IClientStore>();
            _grantedTokenGeneratorHelper = new GrantedTokenGeneratorHelper(
                _simpleIdentityServerConfiguratorStub,
                _jwtGeneratorStub.Object,
                _clientHelperStub.Object,
                _clientRepositoryStub.Object);
        }
    }
}
