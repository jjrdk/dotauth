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
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth;
    using SimpleAuth.WebSite.Authenticate.Actions;
    using Xunit;

    public class ValidateConfirmationCodeActionFixture
    {
        private Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private IValidateConfirmationCodeAction _validateConfirmationCodeAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _validateConfirmationCodeAction.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _validateConfirmationCodeAction.Execute(string.Empty)).ConfigureAwait(false);
        }
        
        [Fact]
        public async Task When_Code_Doesnt_Exist_Then_False_Is_Returned()
        {            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(Task.FromResult((ConfirmationCode)null));

                        var result = await _validateConfirmationCodeAction.Execute("code").ConfigureAwait(false);            Assert.False(result);
        }

        [Fact]
        public async Task When_Code_Is_Expired_Then_False_Is_Returned()
        {            var confirmationCode = new ConfirmationCode
            {
                ExpiresIn = 10,
                IssueAt = DateTime.UtcNow.AddDays(-2)
            };
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(confirmationCode));

                        var result = await _validateConfirmationCodeAction.Execute("code").ConfigureAwait(false);

                        Assert.False(result);
        }

        [Fact]
        public async Task When_Code_Is_Not_Expired_Then_True_Is_Returned()
        {            var confirmationCode = new ConfirmationCode
            {
                ExpiresIn = 200,
                IssueAt = DateTime.UtcNow
            };
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(confirmationCode));

                        var result = await _validateConfirmationCodeAction.Execute("code").ConfigureAwait(false);

                        Assert.True(result);
        }

        private void InitializeFakeObjects()
        {
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _validateConfirmationCodeAction = new ValidateConfirmationCodeAction(_confirmationCodeStoreStub.Object);
        }
    }
}
