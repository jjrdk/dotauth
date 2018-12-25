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

using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Scopes.Actions
{
    using SimpleAuth.Api.Scopes.Actions;
    using SimpleAuth.Errors;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    public class AddScopeOperationFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private IAddScopeOperation _addScopeOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _addScopeOperation.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Scope_Already_Exists_Then_Exception_Is_Thrown()
        {            const string name = "scope_name";
            var scope = new Scope();
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(scope));

            // ACT & ASSERTS
            var ex = await Assert.ThrowsAsync<IdentityServerManagerException>(() => _addScopeOperation.Execute(new Scope
            {
                Name = name
            })).ConfigureAwait(false);
            Assert.NotNull(ex);
            Assert.True(ex.Code == ErrorCodes.InvalidParameterCode);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.TheScopeAlreadyExists, name));
        }

        [Fact]
        public async Task When_AddingScope_Then_Operation_Is_Called()
        {            var parameter = new Scope
            {
                Name = "scope_name"
            };
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(s => s.Get(It.IsAny<string>()))
                .Returns(Task.FromResult((Scope)null));

                        await _addScopeOperation.Execute(parameter).ConfigureAwait(false);

                        _scopeRepositoryStub.Verify(s => s.Insert(parameter));
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _addScopeOperation = new AddScopeOperation(_scopeRepositoryStub.Object);
        }
    }
}
