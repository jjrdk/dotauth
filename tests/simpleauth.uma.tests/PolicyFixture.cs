﻿// Copyright 2016 Habart Thierry
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

namespace SimpleAuth.Uma.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Client.Configuration;
    using Client.Policy;
    using Client.ResourceSet;
    using Shared.DTOs;
    using Xunit;

    public class PolicyFixture : IClassFixture<TestUmaServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private IPolicyClient _policyClient;
        private IResourceSetClient _resourceSetClient;
        private readonly TestUmaServerFixture _server;

        public PolicyFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Add_Policy_And_Pass_No_ResourceIds_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var response = await _policyClient.AddByResolution(new PostPolicy
                    {
                        ResourceSetIds = null
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_request", response.Error.Error);
            Assert.Equal("the parameter resource_set_ids needs to be specified", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Policy_And_Pass_No_Rules_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient.AddByResolution(new PostPolicy
                    {
                        ResourceSetIds = new List<string>
                        {
                            "resource_id"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_request", response.Error.Error);
            Assert.Equal("the parameter rules needs to be specified", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Policy_And_ResourceOwner_Doesnt_Exists_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient.AddByResolution(new PostPolicy
                    {
                        ResourceSetIds = new List<string>
                        {
                            "resource_id"
                        },
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {

                            }
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_resource_set_id", response.Error.Error);
            Assert.Equal("resource set resource_id doesn't exist", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Policy_And_Scope_Doesnt_Exists_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var addResponse = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var response = await _policyClient.AddByResolution(new PostPolicy
                    {
                        ResourceSetIds = new List<string>
                        {
                            addResponse.Content.Id
                        },
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                Scopes = new List<string>
                                {
                                    "scope"
                                }
                            }
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_scope", response.Error.Error);
            Assert.Equal("one or more scopes don't belong to a resource set", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Get_Unknown_Policy_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient
                .GetByResolution("unknown", baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Error);
            Assert.Equal("policy cannot be found", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Policy_And_No_Id_Is_Passed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient.UpdateByResolution(new PutPolicy
                    {

                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_request", response.Error.Error);
            Assert.Equal("the parameter id needs to be specified", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Policy_And_No_Rules_Is_Passed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient.UpdateByResolution(new PutPolicy
                    {
                        PolicyId = "policy"
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_request", response.Error.Error);
            Assert.Equal("the parameter rules needs to be specified", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Unknown_Policy_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient.UpdateByResolution(new PutPolicy
                    {
                        PolicyId = "policy",
                        Rules = new List<PutPolicyRule>
                        {
                            new PutPolicyRule
                            {

                            }
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Error);
            Assert.Equal("policy cannot be found", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Policy_And_Scope_Doesnt_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var addResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var addResponse = await _policyClient.AddByResolution(new PostPolicy
                    {
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new List<PostClaim>
                                {
                                    new PostClaim
                                    {
                                        Type = "role",
                                        Value = "administrator"
                                    }
                                },
                                Scopes = new List<string>
                                {
                                    "read"
                                }
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            addResource.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var response = await _policyClient.UpdateByResolution(new PutPolicy
                    {
                        PolicyId = addResponse.Content.PolicyId,
                        Rules = new List<PutPolicyRule>
                        {
                            new PutPolicyRule
                            {
                                Scopes = new List<string>
                                {
                                    "invalid_scope"
                                }
                            }
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_scope", response.Error.Error);
            Assert.Equal("one or more scopes don't belong to a resource set", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Remove_Unknown_Policy_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient
                .DeleteByResolution("unknown", baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Error);
            Assert.Equal("policy cannot be found", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Resource_And_Pass_No_Resources_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient.AddResourceByResolution("id",
                    new PostAddResourceSet
                    {
                        ResourceSets = null
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_request", response.Error.Error);
            Assert.Equal("the parameter resources needs to be specified", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Resource_And_Pass_Unknown_Policy_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient.AddResourceByResolution("id",
                    new PostAddResourceSet
                    {
                        ResourceSets = new List<string>
                        {
                            "resource"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Error);
            Assert.Equal("policy cannot be found", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Resource_And_ResourceSet_Is_Unknown_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var addResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
                    {
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Scopes = new List<string>
                                {
                                    "read"
                                }
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            addResource.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var response = await _policyClient.AddResourceByResolution(addPolicy.Content.PolicyId,
                    new PostAddResourceSet
                    {
                        ResourceSets = new List<string>
                        {
                            "resource"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("invalid_resource_set_id", response.Error.Error);
            Assert.Equal("resource set resource doesn't exist", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Remove_Resource_And_Pass_Unknown_Policy_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();


            var response = await _policyClient
                .DeleteResourceByResolution("unknown",
                    "resource_id",
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Error);
            Assert.Equal("policy cannot be found", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Remove_Resource_And_Pass_Unknown_Resource_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var addResponse = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
                    {
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Scopes = new List<string>
                                {
                                    "read"
                                }
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            addResponse.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var response = await _policyClient.DeleteResourceByResolution(addPolicy.Content.PolicyId,
                    "resource_id",
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.ContainsError);
            Assert.Equal("invalid_resource_set_id", response.Error.Error);
            Assert.Equal("resource set resource_id doesn't exist", response.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Adding_Policy_Then_Information_Can_Be_Returned()
        {
            InitializeFakeObjects();

            var addResponse = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var response = await _policyClient.AddByResolution(new PostPolicy
                    {
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new List<PostClaim>
                                {
                                    new PostClaim
                                    {
                                        Type = "role",
                                        Value = "administrator"
                                    }
                                },
                                Scopes = new List<string>
                                {
                                    "read"
                                }
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            addResponse.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var information = await _policyClient
                .GetByResolution(response.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.False(string.IsNullOrWhiteSpace(response.Content.PolicyId));
            Assert.NotNull(information);
            Assert.True(information.Content.Rules.Count() == 1);
            Assert.True(information.Content.ResourceSetIds.Count() == 1 &&
                        information.Content.ResourceSetIds.First() == addResponse.Content.Id);
            var rule = information.Content.Rules.First();
            Assert.False(rule.IsResourceOwnerConsentNeeded);
            Assert.True(rule.Claims.Count() == 1);
            Assert.True(rule.Scopes.Count() == 1);
        }

        [Fact]
        public async Task When_Getting_All_Policies_Then_Identifiers_Are_Returned()
        {
            InitializeFakeObjects();

            var addResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
                    {
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new List<PostClaim>
                                {
                                    new PostClaim
                                    {
                                        Type = "role",
                                        Value = "administrator"
                                    }
                                },
                                Scopes = new List<string>
                                {
                                    "read"
                                }
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            addResource.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var response = await _policyClient.GetAllByResolution(baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.Contains(response.Content, r => r == addPolicy.Content.PolicyId);
        }

        [Fact]
        public async Task When_Removing_Policy_Then_Information_Doesnt_Exist()
        {
            InitializeFakeObjects();

            var addResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
                    {
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new List<PostClaim>
                                {
                                    new PostClaim
                                    {
                                        Type = "role",
                                        Value = "administrator"
                                    }
                                },
                                Scopes = new List<string>
                                {
                                    "read"
                                }
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            addResource.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var isRemoved = await _policyClient
                .DeleteByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);
            var ex = await _policyClient
                .GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);

            Assert.False(isRemoved.ContainsError);
            Assert.True(ex.ContainsError);
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task When_Adding_Resource_To_Policy_Then_Changes_Are_Persisted()
        {
            InitializeFakeObjects();

            var firstResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var secondResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
                    {
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new List<PostClaim>
                                {
                                    new PostClaim
                                    {
                                        Type = "role",
                                        Value = "administrator"
                                    }
                                },
                                Scopes = new List<string>
                                {
                                    "read"
                                }
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            firstResource.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var isUpdated = await _policyClient.AddResourceByResolution(addPolicy.Content.PolicyId,
                    new PostAddResourceSet
                    {
                        ResourceSets = new List<string>
                        {
                            secondResource.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var information = await _policyClient
                .GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);

            Assert.False(isUpdated.ContainsError);
            Assert.NotNull(information);
            Assert.True(information.Content.ResourceSetIds.Count() == 2 &&
                        information.Content.ResourceSetIds.All(r =>
                            r == firstResource.Content.Id || r == secondResource.Content.Id));
        }

        [Fact]
        public async Task When_Removing_Resource_From_Policy_Then_Changes_Are_Persisted()
        {
            InitializeFakeObjects();

            var firstResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var secondResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
                    {
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new List<PostClaim>
                                {
                                    new PostClaim
                                    {
                                        Type = "role",
                                        Value = "administrator"
                                    }
                                },
                                Scopes = new List<string>
                                {
                                    "read"
                                }
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            firstResource.Content.Id,
                            secondResource.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var isUpdated = await _policyClient.DeleteResourceByResolution(addPolicy.Content.PolicyId,
                    secondResource.Content.Id,
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var information = await _policyClient
                .GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);

            Assert.False(isUpdated.ContainsError);
            Assert.NotNull(information);
            Assert.True(information.Content.ResourceSetIds.Count() == 1 &&
                        information.Content.ResourceSetIds.First() == firstResource.Content.Id);
        }

        [Fact]
        public async Task When_Updating_Policy_Then_Changes_Are_Persisted()
        {
            InitializeFakeObjects();

            var firstResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read",
                            "write"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var secondResource = await _resourceSetClient.AddByResolution(new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read",
                            "write"
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _policyClient.AddByResolution(new PostPolicy
                    {
                        Rules = new List<PostPolicyRule>
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new List<PostClaim>
                                {
                                    new PostClaim
                                    {
                                        Type = "role",
                                        Value = "administrator"
                                    }
                                },
                                Scopes = new List<string>
                                {
                                    "read"
                                }
                            }
                        },
                        ResourceSetIds = new List<string>
                        {
                            firstResource.Content.Id,
                            secondResource.Content.Id
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var firstInfo = await _policyClient
                .GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);

            var isUpdated = await _policyClient.UpdateByResolution(new PutPolicy
                    {
                        PolicyId = firstInfo.Content.Id,
                        Rules = new List<PutPolicyRule>
                        {
                            new PutPolicyRule
                            {
                                Id = firstInfo.Content.Rules.First().Id,
                                Claims = new List<PostClaim>
                                {
                                    new PostClaim
                                    {
                                        Type = "role",
                                        Value = "administrator"
                                    },
                                    new PostClaim
                                    {
                                        Type = "role",
                                        Value = "other"
                                    }
                                },
                                Scopes = new List<string>
                                {
                                    "read",
                                    "write"
                                }
                            }
                        }
                    },
                    baseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            var updatedInformation = await _policyClient
                .GetByResolution(addPolicy.Content.PolicyId, baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);


            Assert.False(isUpdated.ContainsError);
            Assert.NotNull(updatedInformation);
            Assert.True(updatedInformation.Content.Rules.Count() == 1);
            var rule = updatedInformation.Content.Rules.First();
            Assert.True(rule.Claims.Count() == 2);
            Assert.True(rule.Scopes.Count() == 2);
        }

        private void InitializeFakeObjects()
        {
            _policyClient = new PolicyClient(new AddPolicyOperation(_server.Client),
                new GetPolicyOperation(_server.Client),
                new DeletePolicyOperation(_server.Client),
                new GetPoliciesOperation(_server.Client),
                new AddResourceToPolicyOperation(_server.Client),
                new DeleteResourceFromPolicyOperation(_server.Client),
                new UpdatePolicyOperation(_server.Client),
                new GetConfigurationOperation(_server.Client),
                new SearchPoliciesOperation(_server.Client));
            _resourceSetClient = new ResourceSetClient(new AddResourceSetOperation(_server.Client),
                new DeleteResourceSetOperation(_server.Client),
                new GetResourcesOperation(_server.Client),
                new GetResourceOperation(_server.Client),
                new UpdateResourceOperation(_server.Client),
                new GetConfigurationOperation(_server.Client),
                new SearchResourcesOperation(_server.Client));
        }
    }
}
