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

using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleIdentityServer.Core.Api.UserInfo.Actions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.UserInfo
{
    using Jwt;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    public sealed class GetJwsPayloadFixture
    {
        private Mock<IGrantedTokenValidator> _grantedTokenValidatorFake;
        private Mock<ITokenStore> _tokenStoreFake;
        private Mock<IJwtGenerator> _jwtGeneratorFake;
        private Mock<IClientStore> _clientRepositoryFake;
        private IGetJwsPayload _getJwsPayload;
        
        [Fact]
        public async Task When_Pass_Empty_Access_Token_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _getJwsPayload.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Access_Token_Is_Not_Valid_Then_Authorization_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _grantedTokenValidatorFake.Setup(g => g.CheckAccessTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new GrantedTokenValidationResult { IsValid = false }));

                        await Assert.ThrowsAsync<AuthorizationException>(() => _getJwsPayload.Execute("access_token")).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _grantedTokenValidatorFake.Setup(g => g.CheckAccessTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new GrantedTokenValidationResult { IsValid = true }));
            _tokenStoreFake.Setup(g => g.GetAccessToken(It.IsAny<string>()))
                .Returns(Task.FromResult(new GrantedToken { ClientId = "client_id" }));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Client)null));

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getJwsPayload.Execute("access_token")).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidToken);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientIdDoesntExist, "client_id"));
        }

        [Fact]
        public async Task When_Not_ResourceOwner_Token_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _grantedTokenValidatorFake.Setup(g => g.CheckAccessTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new GrantedTokenValidationResult { IsValid = true }));
            _tokenStoreFake.Setup(g => g.GetAccessToken(It.IsAny<string>()))
                .Returns(Task.FromResult(new GrantedToken { ClientId = "client_id" }));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new Client()));

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getJwsPayload.Execute("access_token")).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidToken);
            Assert.True(exception.Message == ErrorDescriptions.TheTokenIsNotAValidResourceOwnerToken);
        }

        [Fact]
        public async Task When_None_Is_Specified_Then_JwsPayload_Is_Returned()
        {            InitializeFakeObjects();
            var grantedToken = new GrantedToken
            {
                UserInfoPayLoad = new JwsPayload()
            };
            var client = new Client
            {
                UserInfoSignedResponseAlg = JwtConstants.JwsAlgNames.NONE
            };
            _grantedTokenValidatorFake.Setup(g => g.CheckAccessTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new GrantedTokenValidationResult { IsValid = true }));
            _tokenStoreFake.Setup(g => g.GetAccessToken(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

                        var result = await _getJwsPayload.Execute("access_token").ConfigureAwait(false);

                        Assert.NotNull(result);
        }

        [Fact]
        public async Task When_There_Is_No_Algorithm_Specified_Then_JwsPayload_Is_Returned()
        {            InitializeFakeObjects();
            var grantedToken = new GrantedToken
            {
                UserInfoPayLoad = new JwsPayload()
            };
            var client = new Client
            {
                UserInfoSignedResponseAlg = string.Empty
            };
            _grantedTokenValidatorFake.Setup(g => g.CheckAccessTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new GrantedTokenValidationResult { IsValid = true }));
            _tokenStoreFake.Setup(g => g.GetAccessToken(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

                        var result = await _getJwsPayload.Execute("access_token").ConfigureAwait(false);

                        Assert.NotNull(result);
        }

        [Fact]
        public async Task When_Algorithms_For_Sign_And_Encrypt_Are_Specified_Then_Functions_Are_Called()
        {            InitializeFakeObjects();
            const string jwt = "jwt";
            var grantedToken = new GrantedToken
            {
                UserInfoPayLoad = new JwsPayload()
            };
            var client = new Client
            {
                UserInfoSignedResponseAlg = JwtConstants.JwsAlgNames.RS256,
                UserInfoEncryptedResponseAlg = JwtConstants.JweAlgNames.RSA1_5
            };
            _grantedTokenValidatorFake.Setup(g => g.CheckAccessTokenAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new GrantedTokenValidationResult { IsValid = true }));
            _tokenStoreFake.Setup(g => g.GetAccessToken(It.IsAny<string>()))
                .Returns(Task.FromResult(grantedToken));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<string>(),
                It.IsAny<JweAlg>(),
                It.IsAny<JweEnc>()))
                .Returns(Task.FromResult(jwt));

                        var result = await _getJwsPayload.Execute("access_token").ConfigureAwait(false);

                        _jwtGeneratorFake.Verify(j => j.SignAsync(It.IsAny<JwsPayload>(), It.IsAny<JwsAlg>()));
            _jwtGeneratorFake.Verify(j => j.EncryptAsync(It.IsAny<string>(),
                It.IsAny<JweAlg>(),
                JweEnc.A128CBC_HS256));
            var actionResult = (ContentResult)result;
            var contentType = actionResult.ContentType;
            Assert.NotNull(contentType);
            Assert.True(contentType == "application/jwt");
        }

        private void InitializeFakeObjects()
        {
            _grantedTokenValidatorFake = new Mock<IGrantedTokenValidator>();
            _tokenStoreFake = new Mock<ITokenStore>();
            _jwtGeneratorFake = new Mock<IJwtGenerator>();
            _clientRepositoryFake = new Mock<IClientStore>();
            _getJwsPayload = new GetJwsPayload(
                _grantedTokenValidatorFake.Object,
                _jwtGeneratorFake.Object,
                _clientRepositoryFake.Object,
                _tokenStoreFake.Object);
        }
    }
}
