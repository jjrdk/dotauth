// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Server.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using Xunit;

    public class PolicyFixture
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownUma2Configuration = "/.well-known/uma2-configuration";
        private readonly UmaClient _umaClient;
        private readonly TestUmaServerFixture _server;

        public PolicyFixture()
        {
            _server = new TestUmaServerFixture();
            _umaClient = UmaClient.Create(_server.Client, new Uri(BaseUrl + WellKnownUma2Configuration)).Result;
        }

        [Fact]
        public async Task When_Add_Policy_And_Pass_No_ResourceIds_Then_Error_Is_Returned()
        {
            var response = await _umaClient.AddPolicy(new PostPolicy {ResourceSetIds = null}, "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, response.Error.Title);
            Assert.Equal("the parameter resource_set_ids needs to be specified", response.Error.Detail);
        }

        [Fact]
        public async Task When_Add_Policy_And_Pass_No_Rules_Then_Error_Is_Returned()
        {
            var response = await _umaClient.AddPolicy(new PostPolicy {ResourceSetIds = new[] {"resource_id"}}, "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, response.Error.Title);
            Assert.Equal("the parameter rules needs to be specified", response.Error.Detail);
        }

        [Fact]
        public async Task When_Add_Policy_And_ResourceOwner_Does_Not_Exists_Then_Error_Is_Returned()
        {
            var response = await _umaClient.AddPolicy(
                    new PostPolicy {ResourceSetIds = new[] {"resource_id"}, Rules = new[] {new PostPolicyRule { }}},
                    "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("invalid_resource_set_id", response.Error.Title);
            Assert.Equal("resource set resource_id doesn't exist", response.Error.Detail);
        }

        [Fact]
        public async Task When_Add_Policy_And_Scope_Does_Not_Exists_Then_Error_Is_Returned()
        {
            var addResponse = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);

            var response = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        ResourceSetIds = new[] {addResponse.Content.Id},
                        Rules = new[] {new PostPolicyRule {Scopes = new[] {"scope"}}}
                    },
                    "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("invalid_scope", response.Error.Title);
            Assert.Equal("one or more scopes don't belong to a resource set", response.Error.Detail);
        }

        [Fact]
        public async Task When_Get_Unknown_Policy_Then_Error_Is_Returned()
        {
            var response = await _umaClient.GetPolicy("unknown", "header").ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Title);
            Assert.Equal("policy cannot be found", response.Error.Detail);
        }

        [Fact]
        public async Task When_Update_Policy_And_No_Id_Is_Passed_Then_Error_Is_Returned()
        {
            var response = await _umaClient.UpdatePolicy(new PutPolicy { }, "header").ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, response.Error.Title);
            Assert.Equal("the parameter id needs to be specified", response.Error.Detail);
        }

        [Fact]
        public async Task When_Update_Policy_And_No_Rules_Is_Passed_Then_Error_Is_Returned()
        {
            var response = await _umaClient.UpdatePolicy(new PutPolicy {PolicyId = "policy"}, "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, response.Error.Title);
            Assert.Equal("the parameter rules needs to be specified", response.Error.Detail);
        }

        [Fact]
        public async Task When_Update_Unknown_Policy_Then_Error_Is_Returned()
        {
            var response = await _umaClient.UpdatePolicy(
                    new PutPolicy {PolicyId = "policy", Rules = new[] {new PutPolicyRule { }}},
                    "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Title);
            Assert.Equal("policy cannot be found", response.Error.Detail);
        }

        [Fact]
        public async Task When_Update_Policy_And_Scope_Does_Not_Exist_Then_Error_Is_Returned()
        {
            var addResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var addResponse = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        Rules = new[]
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new[] {new PostClaim {Type = "role", Value = "administrator"}},
                                Scopes = new[] {"read"}
                            }
                        },
                        ResourceSetIds = new[] {addResource.Content.Id}
                    },
                    "header")
                .ConfigureAwait(false);

            var response = await _umaClient.UpdatePolicy(
                    new PutPolicy
                    {
                        PolicyId = addResponse.Content.PolicyId,
                        Rules = new[] {new PutPolicyRule {Scopes = new[] {"invalid_scope"}}}
                    },
                    "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("invalid_scope", response.Error.Title);
            Assert.Equal("one or more scopes don't belong to a resource set", response.Error.Detail);
        }

        [Fact]
        public async Task When_Remove_Unknown_Policy_Then_Error_Is_Returned()
        {
            var response = await _umaClient.DeletePolicy("unknown", "header").ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Title);
            Assert.Equal("policy cannot be found", response.Error.Detail);
        }

        [Fact]
        public async Task When_Add_Resource_And_Pass_No_Resources_Then_Error_Is_Returned()
        {
            var response = await _umaClient.AddResource("id", new PostAddResourceSet {ResourceSets = null}, "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, response.Error.Title);
            Assert.Equal("The parameter resources needs to be specified", response.Error.Detail);
        }

        [Fact]
        public async Task When_Add_Resource_And_Pass_Unknown_Policy_Then_Error_Is_Returned()
        {
            var response = await _umaClient.AddResource(
                    "id",
                    new PostAddResourceSet {ResourceSets = new[] {"resource"}},
                    "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Title);
            Assert.Equal("policy cannot be found", response.Error.Detail);
        }

        [Fact]
        public async Task When_Add_Resource_And_ResourceSet_Is_Unknown_Then_Error_Is_Returned()
        {
            var addResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        Rules = new[]
                        {
                            new PostPolicyRule {IsResourceOwnerConsentNeeded = false, Scopes = new[] {"read"}}
                        },
                        ResourceSetIds = new[] {addResource.Content.Id}
                    },
                    "header")
                .ConfigureAwait(false);

            var response = await _umaClient.AddResource(
                    addPolicy.Content.PolicyId,
                    new PostAddResourceSet {ResourceSets = new[] {"resource"}},
                    "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("invalid_resource_set_id", response.Error.Title);
            Assert.Equal("resource set resource doesn't exist", response.Error.Detail);
        }

        [Fact]
        public async Task When_Remove_Resource_And_Pass_Unknown_Policy_Then_Error_Is_Returned()
        {
            var response = await _umaClient.DeletePolicyResource("unknown", "resource_id", "header").ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("not_found", response.Error.Title);
            Assert.Equal("policy cannot be found", response.Error.Detail);
        }

        [Fact]
        public async Task When_Remove_Resource_And_Pass_Unknown_Resource_Then_Error_Is_Returned()
        {
            var addResponse = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        Rules = new[]
                        {
                            new PostPolicyRule {IsResourceOwnerConsentNeeded = false, Scopes = new[] {"read"}}
                        },
                        ResourceSetIds = new[] {addResponse.Content.Id}
                    },
                    "header")
                .ConfigureAwait(false);

            var response = await _umaClient.DeletePolicyResource(addPolicy.Content.PolicyId, "resource_id", "header")
                .ConfigureAwait(false);

            Assert.True(response.ContainsError);
            Assert.Equal("invalid_resource_set_id", response.Error.Title);
            Assert.Equal("resource set resource_id doesn't exist", response.Error.Detail);
        }

        [Fact]
        public async Task When_Adding_Policy_Then_Information_Can_Be_Returned()
        {
            var addResponse = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);

            var response = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        Rules = new[]
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new[] {new PostClaim {Type = "role", Value = "administrator"}},
                                Scopes = new[] {"read"}
                            }
                        },
                        ResourceSetIds = new[] {addResponse.Content.Id}
                    },
                    "header")
                .ConfigureAwait(false);
            var information = await _umaClient.GetPolicy(response.Content.PolicyId, "header").ConfigureAwait(false);

            Assert.False(string.IsNullOrWhiteSpace(response.Content.PolicyId));
            Assert.Single(information.Content.Rules);
            Assert.Equal(addResponse.Content.Id, information.Content.ResourceSetIds.Single());
            var rule = information.Content.Rules.First();
            Assert.False(rule.IsResourceOwnerConsentNeeded);
            Assert.Single(rule.Claims);
            Assert.Single(rule.Scopes);
        }

        [Fact]
        public async Task When_Getting_All_Policies_Then_Identifiers_Are_Returned()
        {
            var addResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        Rules = new[]
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new[] {new PostClaim {Type = "role", Value = "administrator"}},
                                Scopes = new[] {"read"}
                            }
                        },
                        ResourceSetIds = new[] {addResource.Content.Id}
                    },
                    "header")
                .ConfigureAwait(false);

            var response = await _umaClient.GetAllPolicies("header").ConfigureAwait(false);

            Assert.Contains(response.Content, r => r == addPolicy.Content.PolicyId);
        }

        [Fact]
        public async Task When_Removing_Policy_Then_Information_Does_Not_Exist()
        {
            var addResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        Rules = new[]
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new[] {new PostClaim {Type = "role", Value = "administrator"}},
                                Scopes = new[] {"read"}
                            }
                        },
                        ResourceSetIds = new[] {addResource.Content.Id}
                    },
                    "header")
                .ConfigureAwait(false);

            var isRemoved = await _umaClient.DeletePolicy(addPolicy.Content.PolicyId, "header").ConfigureAwait(false);
            var ex = await _umaClient.GetPolicy(addPolicy.Content.PolicyId, "header").ConfigureAwait(false);

            Assert.False(isRemoved.ContainsError);
            Assert.True(ex.ContainsError);
        }

        [Fact]
        public async Task When_Adding_Resource_To_Policy_Then_Changes_Are_Persisted()
        {
            var firstResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var secondResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        Rules = new[]
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new[] {new PostClaim {Type = "role", Value = "administrator"}},
                                Scopes = new[] {"read"}
                            }
                        },
                        ResourceSetIds = new[] {firstResource.Content.Id}
                    },
                    "header")
                .ConfigureAwait(false);

            var isUpdated = await _umaClient.AddResource(
                    addPolicy.Content.PolicyId,
                    new PostAddResourceSet {ResourceSets = new[] {secondResource.Content.Id}},
                    "header")
                .ConfigureAwait(false);
            var information = await _umaClient.GetPolicy(
                    addPolicy.Content.PolicyId,
                    "header")
                .ConfigureAwait(false);

            Assert.False(isUpdated.ContainsError);
            Assert.Equal(2, information.Content.ResourceSetIds.Length);
            Assert.All(
                information.Content.ResourceSetIds,
                r => Assert.True(r == firstResource.Content.Id || r == secondResource.Content.Id));
        }

        [Fact]
        public async Task When_Removing_Resource_From_Policy_Then_Changes_Are_Persisted()
        {
            var firstResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var secondResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        Rules = new[]
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new[] {new PostClaim {Type = "role", Value = "administrator"}},
                                Scopes = new[] {"read"}
                            }
                        },
                        ResourceSetIds = new[] {firstResource.Content.Id, secondResource.Content.Id}
                    },
                    "header")
                .ConfigureAwait(false);

            var isUpdated = await _umaClient.DeletePolicyResource(
                    addPolicy.Content.PolicyId,
                    secondResource.Content.Id,
                    "header")
                .ConfigureAwait(false);
            var information = await _umaClient.GetPolicy(addPolicy.Content.PolicyId, "header").ConfigureAwait(false);

            Assert.False(isUpdated.ContainsError);
            Assert.Equal(firstResource.Content.Id, information.Content.ResourceSetIds.Single());
        }

        [Fact]
        public async Task When_Updating_Policy_Then_Changes_Are_Persisted()
        {
            var firstResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read", "write"}},
                    "header")
                .ConfigureAwait(false);
            var secondResource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "picture", Scopes = new[] {"read", "write"}},
                    "header")
                .ConfigureAwait(false);
            var addPolicy = await _umaClient.AddPolicy(
                    new PostPolicy
                    {
                        Rules = new[]
                        {
                            new PostPolicyRule
                            {
                                IsResourceOwnerConsentNeeded = false,
                                Claims = new[] {new PostClaim {Type = "role", Value = "administrator"}},
                                Scopes = new[] {"read"}
                            }
                        },
                        ResourceSetIds = new[] {firstResource.Content.Id, secondResource.Content.Id}
                    },
                    "header")
                .ConfigureAwait(false);
            var firstInfo = await _umaClient.GetPolicy(addPolicy.Content.PolicyId, "header").ConfigureAwait(false);

            var isUpdated = await _umaClient.UpdatePolicy(
                    new PutPolicy
                    {
                        PolicyId = firstInfo.Content.Id,
                        Rules = new[]
                        {
                            new PutPolicyRule
                            {
                                Id = firstInfo.Content.Rules.First().Id,
                                Claims = new[]
                                {
                                    new PostClaim {Type = "role", Value = "administrator"},
                                    new PostClaim {Type = "role", Value = "other"}
                                },
                                Scopes = new[] {"read", "write"}
                            }
                        }
                    },
                    "header")
                .ConfigureAwait(false);
            var updatedInformation =
                await _umaClient.GetPolicy(addPolicy.Content.PolicyId, "header").ConfigureAwait(false);

            Assert.False(isUpdated.ContainsError);
            Assert.Single(updatedInformation.Content.Rules);
            var rule = updatedInformation.Content.Rules.First();
            Assert.Equal(2, rule.Claims.Length);
            Assert.Equal(2, rule.Scopes.Length);
        }
    }
}
