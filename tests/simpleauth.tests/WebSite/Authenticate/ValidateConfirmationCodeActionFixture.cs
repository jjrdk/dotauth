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
    using Moq;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.WebSite.Authenticate;
    using Xunit;

    public class ValidateConfirmationCodeActionFixture
    {
        private readonly Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private readonly ValidateConfirmationCodeAction _validateConfirmationCodeAction;

        public ValidateConfirmationCodeActionFixture()
        {
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _validateConfirmationCodeAction = new ValidateConfirmationCodeAction(_confirmationCodeStoreStub.Object);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Returns_False()
        {
            var result = await _validateConfirmationCodeAction.Execute(null, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task When_Passing_Empty_Parameter_Then_Returns_False()
        {
            var result = await _validateConfirmationCodeAction.Execute(string.Empty, string.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }


        [Fact]
        public async Task When_Code_Does_Not_Exist_Then_False_Is_Returned()
        {
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ConfirmationCode)null);

            var result = await _validateConfirmationCodeAction.Execute("code", "test", CancellationToken.None)
                .ConfigureAwait(false);
            Assert.False(result);
        }

        [Fact]
        public async Task When_Code_Is_Expired_Then_False_Is_Returned()
        {
            var confirmationCode = new ConfirmationCode { ExpiresIn = 10, IssueAt = DateTimeOffset.UtcNow.AddDays(-2) };
            _confirmationCodeStoreStub
                .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(confirmationCode);

            var result = await _validateConfirmationCodeAction.Execute("code", "test", CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task When_Code_Is_Not_Expired_Then_True_Is_Returned()
        {
            var confirmationCode = new ConfirmationCode { ExpiresIn = 200, IssueAt = DateTimeOffset.UtcNow };
            _confirmationCodeStoreStub
                .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(confirmationCode);

            var result = await _validateConfirmationCodeAction.Execute("code", "test", CancellationToken.None)
                .ConfigureAwait(false);

            Assert.True(result);
        }
    }
}
