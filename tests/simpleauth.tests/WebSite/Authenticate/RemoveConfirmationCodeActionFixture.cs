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

    public class RemoveConfirmationCodeActionFixture
    {
        private Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private IRemoveConfirmationCodeAction _removeConfirmationCodeAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _removeConfirmationCodeAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Code_Is_Removed_Then_Operation_Is_Called()
        {            InitializeFakeObjects();

                        await _removeConfirmationCodeAction.Execute("code").ConfigureAwait(false);

                        _confirmationCodeStoreStub.Verify(c => c.Remove("code"));
        }

        private void InitializeFakeObjects()
        {
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _removeConfirmationCodeAction = new RemoveConfirmationCodeAction(_confirmationCodeStoreStub.Object);
        }
    }
}
