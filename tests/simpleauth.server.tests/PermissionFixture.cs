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
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Client;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using Xunit;

    public class PermissionFixture : IDisposable
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownUma2Configuration = "/.well-known/uma2-configuration";

        private readonly UmaClient _umaClient;
        private readonly TestUmaServerFixture _server;

        public PermissionFixture()
        {
            _server = new TestUmaServerFixture();
            _umaClient = new UmaClient(_server.Client, new Uri(BaseUrl + WellKnownUma2Configuration));
        }

        [Fact]
        public async Task When_ResourceSetId_Is_Null_Then_Error_Is_Returned()
        {
            var ticket = await _umaClient.RequestPermission(
                    "header",
                    requests: new PermissionRequest {ResourceSetId = string.Empty})
                .ConfigureAwait(false);

            Assert.True(ticket.HasError);
            Assert.Equal(ErrorCodes.InvalidRequest, ticket.Error.Title);
            Assert.Equal("the parameter resource_set_id needs to be specified", ticket.Error.Detail);
        }

        [Fact]
        public async Task When_Scopes_Is_Null_Then_Error_Is_Returned()
        {
            var ticket = await _umaClient.RequestPermission(
                    "header",
                    requests: new PermissionRequest {ResourceSetId = "resource"})
                .ConfigureAwait(false);

            Assert.True(ticket.HasError);
            Assert.Equal(ErrorCodes.InvalidRequest, ticket.Error.Title);
            Assert.Equal(
                string.Format(Strings.TheParameterNeedsToBeSpecified, "scopes"),
                ticket.Error.Detail);
        }

        [Fact]
        public async Task When_Resource_Does_Not_Exist_Then_Error_Is_Returned()
        {
            var ticket = await _umaClient.RequestPermission(
                    "header",
                    requests: new PermissionRequest {ResourceSetId = "resource", Scopes = new[] {"scope"}})
                .ConfigureAwait(false);

            Assert.True(ticket.HasError);
            Assert.Equal(ErrorCodes.InvalidResourceSetId, ticket.Error.Title);
            Assert.Equal(string.Format(Strings.TheResourceSetDoesntExist, "resource"), ticket.Error.Detail);
        }

        [Fact]
        public async Task When_Scopes_Does_Not_Exist_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.AddResource(
                    new ResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);

            var ticket = await _umaClient.RequestPermission(
                    "header",
                    requests: new PermissionRequest
                    {
                        ResourceSetId = resource.Content.Id, Scopes = new[] {"scopescopescope"}
                    })
                .ConfigureAwait(false);

            Assert.True(ticket.HasError);
            Assert.Equal("invalid_scope", ticket.Error.Title);
            Assert.Equal("one or more scopes are not valid", ticket.Error.Detail);
        }

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
        {
            var resource = await _umaClient.AddResource(
                    new ResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);

            var ticket = await _umaClient.RequestPermission(
                    "header",
                    requests: new PermissionRequest {ResourceSetId = resource.Content.Id, Scopes = new[] {"read"}})
                .ConfigureAwait(false);

            Assert.NotEmpty(ticket.Content.TicketId);
        }

        [Fact]
        public async Task When_Adding_Permissions_Then_TicketIds_Is_Returned()
        {
            var resource = await _umaClient.AddResource(
                    new ResourceSet {Name = "picture", Scopes = new[] {"read"}},
                    "header")
                .ConfigureAwait(false);
            var permissions = new[]
            {
                new PermissionRequest {ResourceSetId = resource.Content.Id, Scopes = new[] {"read"}},
                new PermissionRequest {ResourceSetId = resource.Content.Id, Scopes = new[] {"read"}}
            };

            var ticket = await _umaClient.RequestPermission("header", CancellationToken.None, permissions)
                .ConfigureAwait(false);

            Assert.NotNull(ticket);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
