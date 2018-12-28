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

namespace SimpleAuth.Tests.WebSite.User
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.WebSite.User.Actions;
    using Xunit;

    public class RemoveConsentOperationFixture
    {
        private Mock<IConsentRepository> _consentRepositoryStub;
        private IRemoveConsentOperation _removeConsentOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

            
            await Assert.ThrowsAsync<ArgumentNullException>(() => _removeConsentOperation.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Deleting_Consent_Then_Boolean_Is_Returned()
        {            const bool isRemoved = true;
            const string consentId = "consent_id";
            InitializeFakeObjects();
            _consentRepositoryStub.Setup(c => c.DeleteAsync(It.IsAny<Consent>()))
                .Returns(Task.FromResult(isRemoved));

                        var result = await _removeConsentOperation.Execute(consentId).ConfigureAwait(false);

                        Assert.True(result == isRemoved);
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryStub = new Mock<IConsentRepository>();
            _removeConsentOperation = new RemoveConsentOperation(_consentRepositoryStub.Object);
        }
    }
}
