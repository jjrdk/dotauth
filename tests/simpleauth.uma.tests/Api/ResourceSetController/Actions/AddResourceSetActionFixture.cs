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

namespace SimpleAuth.Uma.Tests.Api.ResourceSetController.Actions
{
    using Errors;
    using Exceptions;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Parameters;
    using Repositories;
    using SimpleAuth.Api.ResourceSetController.Actions;
    using SimpleAuth.Shared.Models;
    using Xunit;

    public class AddResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private AddResourceSetAction _addResourceSetAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceSetAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Set_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceSet>()))
                .Returns(() => Task.FromResult(false));

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addResourceSetAction.Execute(addResourceParameter)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheResourceSetCannotBeInserted);
        }

        [Fact]
        public async Task When_ResourceSet_Is_Inserted_Then_Id_Is_Returned()
        {
            InitializeFakeObjects();
            var addResourceParameter = new AddResouceSetParameter
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceSet>()))
                .Returns(Task.FromResult(true));

            var result = await _addResourceSetAction.Execute(addResourceParameter).ConfigureAwait(false);

            Assert.NotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _addResourceSetAction = new AddResourceSetAction(_resourceSetRepositoryStub.Object);
        }
    }
}
