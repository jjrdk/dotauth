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


namespace SimpleAuth.Tests.WebSite.Authenticate
{
    using Exceptions;
    using Moq;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.WebSite.Authenticate;
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class GenerateAndSendCodeActionFixture
    {
        private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private readonly Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private readonly Mock<ITwoFactorAuthenticationHandler> _twoFactorAuthenticationHandlerStub;
        private readonly GenerateAndSendCodeAction _generateAndSendCodeAction;

        public GenerateAndSendCodeActionFixture()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _twoFactorAuthenticationHandlerStub = new Mock<ITwoFactorAuthenticationHandler>();
            _generateAndSendCodeAction = new GenerateAndSendCodeAction(
                _resourceOwnerRepositoryStub.Object,
                _confirmationCodeStoreStub.Object,
                _twoFactorAuthenticationHandlerStub.Object);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => _generateAndSendCodeAction.Send(null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ResourceOwner) null);

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(
                    () => _generateAndSendCodeAction.Send("subject", CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Code);
            Assert.Equal(ErrorMessages.TheRoDoesntExist, exception.Message);
        }

        [Fact]
        public async Task When_Two_Factor_Auth_Is_Not_Enabled_Then_Exception_Is_Thrown()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceOwner {TwoFactorAuthentication = string.Empty});

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(
                    () => _generateAndSendCodeAction.Send("subject", CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Code);
            Assert.Equal(ErrorMessages.TwoFactorAuthenticationIsNotEnabled, exception.Message);
        }

        [Fact]
        public async Task When_ResourceOwner_Does_Not_Have_The_Required_Claim_Then_Exception_Is_Thrown()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        new ResourceOwner
                        {
                            TwoFactorAuthentication = "email",
                            Claims = new[] {new Claim("key", "value")},
                            Subject = "subject"
                        }));
            var fakeAuthService = new Mock<ITwoFactorAuthenticationService>();
            fakeAuthService.SetupGet(f => f.RequiredClaim).Returns("claim");
            _twoFactorAuthenticationHandlerStub.Setup(t => t.Get(It.IsAny<string>())).Returns(fakeAuthService.Object);

            await Assert
                .ThrowsAsync<ClaimRequiredException>(
                    () => _generateAndSendCodeAction.Send("subject", CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Code_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        new ResourceOwner
                        {
                            TwoFactorAuthentication = "email",
                            Claims = new[] {new Claim("key", "value")},
                            Subject = "subject"
                        }));
            var fakeAuthService = new Mock<ITwoFactorAuthenticationService>();
            fakeAuthService.SetupGet(f => f.RequiredClaim).Returns("key");
            _twoFactorAuthenticationHandlerStub.Setup(t => t.Get(It.IsAny<string>())).Returns(fakeAuthService.Object);
            _confirmationCodeStoreStub
                .Setup(r => r.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ConfirmationCode) null);
            _confirmationCodeStoreStub.Setup(r => r.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(
                    () => _generateAndSendCodeAction.Send("subject", CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Code);
            Assert.Equal(ErrorMessages.TheConfirmationCodeCannotBeSaved, exception.Message);
        }

        [Fact]
        public async Task When_Code_Is_Generated_And_Inserted_Then_Handler_Is_Called()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        new ResourceOwner
                        {
                            TwoFactorAuthentication = "email",
                            Claims = new[] {new Claim("key", "value")},
                            Subject = "subject"
                        }));
            var fakeAuthService = new Mock<ITwoFactorAuthenticationService>();
            fakeAuthService.SetupGet(f => f.RequiredClaim).Returns("key");
            _twoFactorAuthenticationHandlerStub.Setup(t => t.Get(It.IsAny<string>())).Returns(fakeAuthService.Object);
            _confirmationCodeStoreStub
                .Setup(r => r.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ConfirmationCode) null);
            _confirmationCodeStoreStub.Setup(r => r.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _generateAndSendCodeAction.Send("subject", CancellationToken.None).ConfigureAwait(false);

            _twoFactorAuthenticationHandlerStub.Verify(
                t => t.SendCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ResourceOwner>()));
        }
    }
}
