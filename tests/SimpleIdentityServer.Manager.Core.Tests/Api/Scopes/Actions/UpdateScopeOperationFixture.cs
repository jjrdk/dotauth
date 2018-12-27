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

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Scopes.Actions
{
    using Moq;
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Api.Scopes.Actions;
    using SimpleAuth.Errors;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Xunit;

    public class UpdateScopeOperationFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private IUpdateScopeOperation _updateScopeOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _updateScopeOperation.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Scope_Doesnt_Exist_Then_Exception_Is_Thrown()
        {            const string name = "scope_name";
            Scope scope = null;
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(scope));

            // ACT & ASSERTS
            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _updateScopeOperation.Execute(new Scope
            {
                Name = name
            })).ConfigureAwait(false);
            Assert.NotNull(ex);
            Assert.True(ex.Code == ErrorCodes.InvalidParameterCode);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.TheScopeDoesntExist, name));
        }

        [Fact]
        public async Task When_Updating_Then_Operation_Is_Called()
        {            var parameter = new Scope
            {
                Name = "scope_name"
            };
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(parameter));

                        await _updateScopeOperation.Execute(parameter).ConfigureAwait(false);

                        _scopeRepositoryStub.Verify(s => s.Update(parameter));
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _updateScopeOperation = new UpdateScopeOperation(_scopeRepositoryStub.Object);
        }
    }
}
