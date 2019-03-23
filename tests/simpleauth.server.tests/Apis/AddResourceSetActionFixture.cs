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

namespace SimpleAuth.Server.Tests.Apis
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth.Api.ResourceSetController;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Xunit;

    public class AddResourceSetActionFixture
    {
        private readonly Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private readonly AddResourceSetAction _addResourceSetAction;

        public AddResourceSetActionFixture()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _addResourceSetAction = new AddResourceSetAction(_resourceSetRepositoryStub.Object);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<NullReferenceException>(() => _addResourceSetAction.Execute(null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Set_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
        {
            var addResourceParameter = new PostResourceSet
            {
                Name = "name",
                Scopes = new[] {"scope"},
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Add(It.IsAny<ResourceSet>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(false));

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(
                    () => _addResourceSetAction.Execute(addResourceParameter, CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(ErrorDescriptions.TheResourceSetCannotBeInserted, exception.Message);
        }

        [Fact]
        public async Task When_ResourceSet_Is_Inserted_Then_Id_Is_Returned()
        {
            var addResourceParameter = new PostResourceSet
            {
                Name = "name",
                Scopes = new[] {"scope"},
                IconUri = "http://localhost",
                Uri = "http://localhost"
            };
            _resourceSetRepositoryStub.Setup(r => r.Add(It.IsAny<ResourceSet>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _addResourceSetAction.Execute(addResourceParameter, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.NotNull(result);
        }
    }
}
