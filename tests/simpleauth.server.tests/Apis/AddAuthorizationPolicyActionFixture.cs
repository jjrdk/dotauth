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
    using SimpleAuth.Api.PolicyController;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Xunit;

    public class AddAuthorizationPolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private AddAuthorizationPolicyAction _addAuthorizationPolicyAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _addAuthorizationPolicyAction.Execute(null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Empty_ResourceSetId_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var addPolicyParameter = new PostPolicy();

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter, CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(
                string.Format(
                    ErrorDescriptions.TheParameterNeedsToBeSpecified,
                    UmaConstants.AddPolicyParameterNames.ResourceSetIds),
                exception.Message);
        }

        [Fact]
        public async Task When_Passing_No_Rules_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new PostPolicy { ResourceSetIds = new[] { resourceSetId } };

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter, CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(
                exception.Message
                == string.Format(
                    ErrorDescriptions.TheParameterNeedsToBeSpecified,
                    UmaConstants.AddPolicyParameterNames.Rules));
        }

        [Fact]
        public async Task When_ResourceSetId_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new PostPolicy
            {
                ResourceSetIds = new[] { resourceSetId },
                Rules = new[]
                {
                    new PostPolicyRule
                    {
                        Scopes = new [] {"invalid_scope"},
                        ClientIdsAllowed = new [] {"client_id"}
                    }
                }
            };

            InitializeFakeObjects();
            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter, CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidResourceSetId, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId), exception.Message);
        }

        [Fact]
        public async Task When_Scope_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new PostPolicy
            {
                ResourceSetIds = new[] { resourceSetId },
                Rules = new[]
                {
                    new PostPolicyRule
                    {
                        Scopes = new [] {"invalid_scope"},
                        ClientIdsAllowed = new [] {"client_id"}
                    }
                }
            };
            var resourceSet = new ResourceSet { Scopes = new[] { "scope" } };

            InitializeFakeObjects(resourceSet);
            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter, CancellationToken.None))
                .ConfigureAwait(false);

            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
        }

        [Fact]
        public async Task When_Adding_AuthorizationPolicy_Then_Id_Is_Returned()
        {
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new PostPolicy
            {
                ResourceSetIds = new[] { resourceSetId },
                Rules = new[]
                {
                    new PostPolicyRule
                    {
                        Scopes = new[] {"scope"},
                        ClientIdsAllowed = new[] {"client_id"},
                        Claims = new[] {new PostClaim {Type = "type", Value = "value"}}
                    }
                }
            };
            var resourceSet = new ResourceSet { Scopes = new[] { "scope" } };

            InitializeFakeObjects(resourceSet);

            var result = await _addAuthorizationPolicyAction.Execute(addPolicyParameter, CancellationToken.None).ConfigureAwait(false);

            Assert.NotNull(result);
        }

        private void InitializeFakeObjects(ResourceSet resourceSet = null)
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _policyRepositoryStub.Setup(x => x.Add(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(resourceSet);
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(new[] { resourceSet });

            _addAuthorizationPolicyAction = new AddAuthorizationPolicyAction(
                _policyRepositoryStub.Object,
                _resourceSetRepositoryStub.Object);
        }
    }
}
