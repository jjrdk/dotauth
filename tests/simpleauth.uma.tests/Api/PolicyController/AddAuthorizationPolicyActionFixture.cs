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

namespace SimpleAuth.Uma.Tests.Api.PolicyController
{
    using Errors;
    using Exceptions;
    using Moq;
    using Parameters;
    using Repositories;
    using SimpleAuth.Api.PolicyController.Actions;
    using SimpleAuth.Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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

            await Assert.ThrowsAsync<ArgumentNullException>(() => _addAuthorizationPolicyAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Empty_ResourceSetId_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var addPolicyParameter = new AddPolicyParameter();

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.AddPolicyParameterNames.ResourceSetIds));
        }

        [Fact]
        public async Task When_Passing_No_Rules_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                }
            };

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.AddPolicyParameterNames.Rules));
        }

        [Fact]
        public async Task When_ResourceSetId_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                Rules = new List<AddPolicyRuleParameter>
                {
                    new AddPolicyRuleParameter
                    {
                        Scopes = new List<string>
                        {
                            "invalid_scope"
                        },
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        }
                    }
                }
            };

            InitializeFakeObjects();
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter)).ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidResourceSetId, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId), exception.Message);
        }

        [Fact]
        public async Task When_Scope_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                Rules = new List<AddPolicyRuleParameter>
                {
                    new AddPolicyRuleParameter
                    {
                        Scopes = new List<string>
                        {
                            "invalid_scope"
                        },
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        }
                    }
                }
            };
            var resourceSet = new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope"
                }
            };

            InitializeFakeObjects(resourceSet);
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addAuthorizationPolicyAction.Execute(addPolicyParameter)).ConfigureAwait(false);

            Assert.True(exception.Code == ErrorCodes.InvalidScope);
            Assert.True(exception.Message == ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
        }

        [Fact]
        public async Task When_Adding_AuthorizationPolicy_Then_Id_Is_Returned()
        {
            const string resourceSetId = "resource_set_id";
            var addPolicyParameter = new AddPolicyParameter
            {
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                Rules = new List<AddPolicyRuleParameter>
                {
                    new AddPolicyRuleParameter
                    {
                        Scopes = new List<string>
                        {
                            "scope"
                        },
                        ClientIdsAllowed = new List<string>
                        {
                            "client_id"
                        },
                        Claims = new List<AddClaimParameter>
                        {
                            new AddClaimParameter
                            {
                                Type = "type",
                                Value = "value"
                            }
                        }
                    }
                }

            };
            var resourceSet = new ResourceSet
            {
                Scopes = new List<string>
                {
                    "scope"
                }
            };

            InitializeFakeObjects(resourceSet);

            var result = await _addAuthorizationPolicyAction.Execute(addPolicyParameter).ConfigureAwait(false);

            Assert.NotNull(result);
        }

        private void InitializeFakeObjects(ResourceSet resourceSet = null)
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _policyRepositoryStub.Setup(x => x.Add(It.IsAny<Policy>())).ReturnsAsync(true);
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(resourceSet);
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new[] { resourceSet });

            _addAuthorizationPolicyAction =
                new AddAuthorizationPolicyAction(_policyRepositoryStub.Object, _resourceSetRepositoryStub.Object);
        }
    }
}
