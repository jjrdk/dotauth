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

namespace SimpleAuth.Tests.Api.Scopes.Actions
{
    using System;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Api.Scopes.Actions;
    using Xunit;

    public class GetScopeOperationFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private IGetScopeOperation _getScopeOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _getScopeOperation.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getScopeOperation.Execute(string.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Scope_Does_Not_Exist_Then_Exception_Is_Thrown()
        {            const string scopeName = "invalid_scope_name";
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns(Task.FromResult((Scope)null));

            
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _getScopeOperation.Execute(scopeName)).ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheScopeDoesntExist, scopeName));
        }

        [Fact]
        public async Task When_Scope_Is_Retrieved_Then_Scope_Is_Returned()
        {            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(new Scope()));

                        await _getScopeOperation.Execute("scope").ConfigureAwait(false);

                        _scopeRepositoryStub.Verify(s => s.Get("scope"));
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _getScopeOperation = new GetScopeOperation(_scopeRepositoryStub.Object);
        }
    }
}
