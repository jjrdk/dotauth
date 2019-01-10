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

namespace SimpleAuth.Uma.Tests
{
    using Client.Configuration;
    using Client.Permission;
    using Client.ResourceSet;
    using MiddleWares;
    using SimpleAuth.Errors;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Shared.DTOs;
    using Xunit;

    public class PermissionFixture : IClassFixture<TestUmaServerFixture>
    {
        private const string BaseUrl = "http://localhost:5000";
        //private IPolicyClient _policyClient;
        private ResourceSetClient _resourceSetClient;
        private PermissionClient _permissionClient;
        private readonly TestUmaServerFixture _server;

        public PermissionFixture(TestUmaServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Client_Is_Not_Authenticated_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var resource = await _resourceSetClient.AddByResolution(
                    new PostResourceSet
                    {
                        Name = "picture",
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            UserStore.Instance().ClientId = null;
            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = resource.Content.Id,
                Scopes = new List<string>
                        {
                            "read"
                        }
            },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);
            UserStore.Instance().ClientId = "client";

            Assert.True(ticket.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, ticket.Error.Error);
            Assert.Equal("the client_id cannot be extracted", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_ResourceSetId_Is_Null_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = string.Empty
            },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.True(ticket.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, ticket.Error.Error);
            Assert.Equal("the parameter resource_set_id needs to be specified", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Scopes_Is_Null_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = "resource"
            },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.True(ticket.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, ticket.Error.Error);
            Assert.Equal("the parameter scopes needs to be specified", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Resource_Does_Not_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = "resource",
                Scopes = new List<string>
                        {
                            "scope"
                        }
            },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_resource_set_id", ticket.Error.Error);
            Assert.Equal("resource set resource doesn't exist", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Scopes_Does_Not_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                        {
                            "read"
                        }
            },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = resource.Content.Id,
                Scopes = new List<string>
                        {
                            "scopescopescope"
                        }
            },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.True(ticket.ContainsError);
            Assert.Equal("invalid_scope", ticket.Error.Error);
            Assert.Equal("one or more scopes are not valid", ticket.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
        {
            InitializeFakeObjects();
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
            {
                Name = "picture",
                Scopes = new List<string>
                        {
                            "read"
                        }
            },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            var ticket = await _permissionClient.AddByResolution(new PostPermission
            {
                ResourceSetId = resource.Content.Id,
                Scopes = new List<string>
                        {
                            "read"
                        }
            },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(ticket);
            Assert.NotEmpty(ticket.Content.TicketId);
        }

        [Fact]
        public async Task When_Adding_Permissions_Then_TicketIds_Is_Returned()
        {
            const string baseUrl = "http://localhost:5000";
            InitializeFakeObjects();
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet
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
            var permissions = new List<PostPermission>
            {
                new PostPermission
                {
                    ResourceSetId = resource.Content.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                },
                new PostPermission
                {
                    ResourceSetId = resource.Content.Id,
                    Scopes = new List<string>
                    {
                        "read"
                    }
                }
            };

            var ticket = await _permissionClient
                .AddByResolution(permissions, baseUrl + "/.well-known/uma2-configuration", "header")
                .ConfigureAwait(false);

            Assert.NotNull(ticket);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetClient = new ResourceSetClient(_server.Client,
                new GetConfigurationOperation(_server.Client));
            _permissionClient = new PermissionClient(_server.Client,
                new GetConfigurationOperation(_server.Client));
        }
    }
}
