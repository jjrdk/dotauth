﻿#region copyright
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
#endregion

using Moq;
using SimpleIdentityServer.Core.Api.Revocation.Actions;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Revocation
{
    public class RevokeTokenActionFixture
    {
        private Mock<IAuthenticateInstructionGenerator> _authenticateInstructionGeneratorStub;

        private Mock<IAuthenticateClient> _authenticateClientStub;

        private Mock<IGrantedTokenRepository> _grantedTokenRepositoryStub;

        private IRevokeTokenAction _revokeTokenAction;

        #region Excceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _revokeTokenAction.Execute(null, null));
            Assert.Throws<ArgumentNullException>(() => _revokeTokenAction.Execute(new RevokeTokenParameter(), null));
        }

        [Fact]
        public void When_Client_Credentials_Are_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RevokeTokenParameter
            {
                Token = "access_token"
            };
            string errorMessage;
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out errorMessage))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _revokeTokenAction.Execute(parameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
        }

        [Fact]
        public void When_Token_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RevokeTokenParameter
            {
                Token = "access_token"
            };
            string errorMessage;
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out errorMessage))
                .Returns(() => new Client());
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>()))
                .Returns(() => null);
            _grantedTokenRepositoryStub.Setup(g => g.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _revokeTokenAction.Execute(parameter, null));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidToken);
            Assert.True(exception.Message == ErrorDescriptions.TheTokenDoesntExist);
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_Invalidating_Refresh_Token_Then_GrantedTokenChildren_Are_Removed()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parent = new GrantedToken
            {
                RefreshToken = "refresh_token"
            };
            var child = new GrantedToken
            {
                ParentRefreshToken = "refresh_token",
                AccessToken = "access_token_child"
            };
            var parameter = new RevokeTokenParameter
            {
                Token = "refresh_token"
            };
            string errorMessage;
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out errorMessage))
                .Returns(() => new Client());
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>()))
                .Returns(() => null);
            _grantedTokenRepositoryStub.Setup(g => g.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(parent);
            _grantedTokenRepositoryStub.Setup(g => g.GetGrantedTokenChildren(It.IsAny<string>()))
                .Returns(new List<GrantedToken>
                {
                    child
                });
            _grantedTokenRepositoryStub.Setup(g => g.Delete(It.IsAny<GrantedToken>()))
                .Returns(true);

            // ACT
            _revokeTokenAction.Execute(parameter, null);

            // ASSERTS
            _grantedTokenRepositoryStub.Verify(g => g.Delete(child));
            _grantedTokenRepositoryStub.Verify(g => g.Update(parent));
        }

        [Fact]
        public void When_Invalidating_Access_Token_Then_GrantedToken_Is_Removed()
        {
            // ARRANGE
            InitializeFakeObjects();
            var grantedToken = new GrantedToken
            {
                AccessToken = "access_token"
            };
            var parameter = new RevokeTokenParameter
            {
                Token = "access_token"
            };
            string errorMessage;
            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
                .Returns(new AuthenticateInstruction());
            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), out errorMessage))
                .Returns(() => new Client());
            _grantedTokenRepositoryStub.Setup(g => g.GetToken(It.IsAny<string>()))
                .Returns(grantedToken);
            _grantedTokenRepositoryStub.Setup(g => g.GetTokenByRefreshToken(It.IsAny<string>()))
                .Returns(() => null);
            _grantedTokenRepositoryStub.Setup(g => g.Delete(It.IsAny<GrantedToken>()))
                .Returns(true);

            // ACT
            _revokeTokenAction.Execute(parameter, null);

            // ASSERTS
            _grantedTokenRepositoryStub.Verify(g => g.Delete(grantedToken));
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _authenticateInstructionGeneratorStub = new Mock<IAuthenticateInstructionGenerator>();
            _authenticateClientStub = new Mock<IAuthenticateClient>();
            _grantedTokenRepositoryStub = new Mock<IGrantedTokenRepository>();
            _revokeTokenAction = new RevokeTokenAction(
                _authenticateInstructionGeneratorStub.Object,
                _authenticateClientStub.Object,
                _grantedTokenRepositoryStub.Object);
        }

        #endregion
    }
}