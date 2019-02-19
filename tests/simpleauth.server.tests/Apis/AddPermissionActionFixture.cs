﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth.Api.PermissionController;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Xunit;

    public class AddPermissionActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<ITicketStore> _ticketStoreStub;
        private RuntimeSettings _configurationServiceStub;
        private AddPermissionAction _addPermissionAction;

        [Fact]
        public async Task When_Passing_No_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects(Array.Empty<ResourceSet>());

            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _addPermissionAction.Execute(null, CancellationToken.None, null))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _addPermissionAction.Execute("client_id", CancellationToken.None, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _addPermissionAction.Execute(null, CancellationToken.None, (PostPermission)null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_RequiredParameter_ResourceSetId_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            const string clientId = "client_id";
            InitializeFakeObjects(Array.Empty<ResourceSet>());
            var addPermissionParameter = new PostPermission();

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _addPermissionAction.Execute(clientId, CancellationToken.None, addPermissionParameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(
                string.Format(
                    ErrorDescriptions.TheParameterNeedsToBeSpecified,
                    UmaConstants.AddPermissionNames.ResourceSetId),
                exception.Message);
        }

        [Fact]
        public async Task When_RequiredParameter_Scopes_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            const string clientId = "client_id";
            InitializeFakeObjects(Array.Empty<ResourceSet>());
            var addPermissionParameter = new PostPermission { ResourceSetId = "resource_set_id" };

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _addPermissionAction.Execute(clientId, CancellationToken.None, addPermissionParameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.AddPermissionNames.Scopes),
                exception.Message);
        }

        [Fact]
        public async Task When_ResourceSet_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects(Array.Empty<ResourceSet>());
            var addPermissionParameter = new PostPermission { ResourceSetId = resourceSetId, Scopes = new[] { "scope" } };

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _addPermissionAction.Execute(clientId, CancellationToken.None, addPermissionParameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidResourceSetId, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId), exception.Message);
        }

        [Fact]
        public async Task When_Scope_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            var addPermissionParameter = new PostPermission
            {
                ResourceSetId = resourceSetId,
                Scopes = new[] { "invalid_scope" }
            };
            var resources = new[] { new ResourceSet { Id = resourceSetId, Scopes = new[] { "scope" } } };
            InitializeFakeObjects(resources);

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _addPermissionAction.Execute(clientId, CancellationToken.None, addPermissionParameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidScope, exception.Code);
            Assert.Equal(ErrorDescriptions.TheScopeAreNotValid, exception.Message);
        }

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
        {
            const string clientId = "client_id";
            const string resourceSetId = "resource_set_id";
            var addPermissionParameter = new PostPermission { ResourceSetId = resourceSetId, Scopes = new[] { "scope" } };
            var resources = new[] { new ResourceSet { Id = resourceSetId, Scopes = new[] { "scope" } } };
            InitializeFakeObjects(resources);
            _ticketStoreStub.Setup(r => r.Add(It.IsAny<Ticket>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _addPermissionAction.Execute(clientId, CancellationToken.None, addPermissionParameter)
                .ConfigureAwait(false);

            Assert.NotEmpty(result);
        }

        private void InitializeFakeObjects(params ResourceSet[] resourceSets)
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(resourceSets.FirstOrDefault);
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<string[]>())).ReturnsAsync(resourceSets);
            _ticketStoreStub = new Mock<ITicketStore>();
            _configurationServiceStub = new RuntimeSettings(ticketLifeTime: TimeSpan.FromSeconds(2));
            _addPermissionAction = new AddPermissionAction(
                _resourceSetRepositoryStub.Object,
                _ticketStoreStub.Object,
                _configurationServiceStub);
        }
    }
}