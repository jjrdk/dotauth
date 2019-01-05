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
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Moq;
    using Services;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.WebSite.Authenticate.Actions;
    using Xunit;

    public class GenerateAndSendCodeActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private Mock<ITwoFactorAuthenticationHandler> _twoFactorAuthenticationHandlerStub;
        private IGenerateAndSendCodeAction _generateAndSendCodeAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _generateAndSendCodeAction.ExecuteAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_Does_Not_Exist_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((ResourceOwner)null));

                        var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _generateAndSendCodeAction.ExecuteAsync("subject")).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Two_Factor_Auth_Is_Not_Enabled_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = string.Empty
                }));

                        var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _generateAndSendCodeAction.ExecuteAsync("subject")).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.TwoFactorAuthenticationIsNotEnabled);
        }

        [Fact]
        public async Task When_ResourceOwner_Does_Not_Have_The_Required_Claim_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = "email",
                    Claims = new List<Claim>
                    {
                        new Claim("key", "value")
                    },
                    Id = "subject"
                }));
            var fakeAuthService = new Mock<ITwoFactorAuthenticationService>();
            fakeAuthService.SetupGet(f => f.RequiredClaim).Returns("claim");
            _twoFactorAuthenticationHandlerStub.Setup(t => t.Get(It.IsAny<string>())).Returns(fakeAuthService.Object);

                        var exception = await Assert.ThrowsAsync<ClaimRequiredException>(() => _generateAndSendCodeAction.ExecuteAsync("subject")).ConfigureAwait(false);

                        Assert.NotNull(exception);
        }

        [Fact]
        public async Task When_Code_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = "email",
                    Claims = new List<Claim>
                    {
                        new Claim("key", "value")
                    },
                    Id = "subject"
                }));
            var fakeAuthService = new Mock<ITwoFactorAuthenticationService>();
            fakeAuthService.SetupGet(f => f.RequiredClaim).Returns("key");
            _twoFactorAuthenticationHandlerStub.Setup(t => t.Get(It.IsAny<string>())).Returns(fakeAuthService.Object);
            _confirmationCodeStoreStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult((ConfirmationCode)null));
            _confirmationCodeStoreStub.Setup(r => r.Add(It.IsAny<ConfirmationCode>()))
                .Returns(Task.FromResult(false));

                        var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _generateAndSendCodeAction.ExecuteAsync("subject")).ConfigureAwait(false);

                        Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.TheConfirmationCodeCannotBeSaved);
        }

        [Fact]
        public async Task When_Code_Is_Generated_And_Inserted_Then_Handler_Is_Called()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    TwoFactorAuthentication = "email",
                    Claims = new List<Claim>
                    {
                        new Claim("key", "value")
                    },
                    Id = "subject"
                }));
            var fakeAuthService = new Mock<ITwoFactorAuthenticationService>();
            fakeAuthService.SetupGet(f => f.RequiredClaim).Returns("key");
            _twoFactorAuthenticationHandlerStub.Setup(t => t.Get(It.IsAny<string>())).Returns(fakeAuthService.Object);
            _confirmationCodeStoreStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult((ConfirmationCode)null));
            _confirmationCodeStoreStub.Setup(r => r.Add(It.IsAny<ConfirmationCode>()))
                .Returns(Task.FromResult(true));

                        await _generateAndSendCodeAction.ExecuteAsync("subject").ConfigureAwait(false);

                        _twoFactorAuthenticationHandlerStub.Verify(t => t.SendCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _twoFactorAuthenticationHandlerStub = new Mock<ITwoFactorAuthenticationHandler>();
            _generateAndSendCodeAction = new GenerateAndSendCodeAction(
                _resourceOwnerRepositoryStub.Object,
                _confirmationCodeStoreStub.Object,
                _twoFactorAuthenticationHandlerStub.Object);
        }
    }
}
