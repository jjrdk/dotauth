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

namespace SimpleAuth.Uma.Tests.Api.PermissionController.Actions
{
    using Errors;
    using Exceptions;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Parameters;
    using Repositories;
    using SimpleAuth.Api.PermissionController.Actions;
    using SimpleAuth.Shared.Models;
    using Uma;
    using Xunit;
    
    public class AddPermissionActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<ITicketStore> _ticketStoreStub;
        private UmaConfigurationOptions _configurationServiceStub;
        private AddPermissionAction _addPermissionAction;

        [Fact]
        public async Task When_Passing_No_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects(Array.Empty<ResourceSet>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => _addPermissionAction.Execute(null, (IEnumerable<AddPermissionParameter>)null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addPermissionAction.Execute("client_id", (IEnumerable<AddPermissionParameter>)null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addPermissionAction.Execute(null, (AddPermissionParameter)null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_RequiredParameter_ResourceSetId_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            const string clientId = "client_id";
            InitializeFakeObjects(Array.Empty<ResourceSet>());
            var addPermissionParameter = new AddPermissionParameter();

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addPermissionAction.Execute(clientId, addPermissionParameter)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.AddPermissionNames.ResourceSetId));
        }

        [Fact]
        public async Task When_RequiredParameter_Scopes_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            const string clientId = "client_id";
            InitializeFakeObjects(Array.Empty<ResourceSet>());
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = "resource_set_id"
            };

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addPermissionAction.Execute(clientId, addPermissionParameter)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.AddPermissionNames.Scopes));
        }

        [Fact]
        public async Task When_ResourceSet_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects(Array.Empty<ResourceSet>());
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addPermissionAction.Execute(clientId, addPermissionParameter)).ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
        }

        [Fact]
        public async Task When_Scope_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "invalid_scope"
                }
            };
            IEnumerable<ResourceSet> resources = new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = resourceSetId,
                    Scopes = new List<string>
                    {
                        "scope"
                    }
                }
            };
            InitializeFakeObjects(resources);

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addPermissionAction.Execute(clientId, addPermissionParameter)).ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == ErrorDescriptions.TheScopeAreNotValid);
        }

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
        {
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            var addPermissionParameter = new AddPermissionParameter
            {
                ResourceSetId = resourceSetId,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            IEnumerable<ResourceSet> resources = new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = resourceSetId,
                    Scopes = new List<string>
                    {
                        "scope"
                    }
                }
            };
            InitializeFakeObjects(resources);
            _ticketStoreStub.Setup(r => r.AddAsync(It.IsAny<Ticket>())).Returns(Task.FromResult(true));
            
            var result = await _addPermissionAction.Execute(clientId, addPermissionParameter).ConfigureAwait(false);

            Assert.NotEmpty(result);
        }

        private void InitializeFakeObjects(IEnumerable<ResourceSet> resourceSets)
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(resourceSets.FirstOrDefault);
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<IEnumerable<string>>())).ReturnsAsync(resourceSets);
            _ticketStoreStub = new Mock<ITicketStore>();
            _configurationServiceStub = new UmaConfigurationOptions(ticketLifetime: TimeSpan.FromSeconds(2));
            _addPermissionAction = new AddPermissionAction(
                _resourceSetRepositoryStub.Object,
                _ticketStoreStub.Object,
                _configurationServiceStub);
        }
    }
}
