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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Api.Scopes.Actions;
    using Xunit;

    public class GetScopesOperationFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private IGetScopesOperation _getScopesOperation;
        
        [Fact]
        public async Task When_Executing_Operation_Then_Operation_Is_Called()
        {            ICollection<Scope> scopes = new List<Scope>();
            InitializeFakeObjects();
            _scopeRepositoryStub.Setup(c => c.GetAll())
                .Returns(Task.FromResult(scopes));

                        await _getScopesOperation.Execute().ConfigureAwait(false);

                        _scopeRepositoryStub.Verify(c => c.GetAll());
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _getScopesOperation = new GetScopesOperation(_scopeRepositoryStub.Object);
        }
    }
}
