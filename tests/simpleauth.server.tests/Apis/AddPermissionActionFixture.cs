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
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Divergic.Logging.Xunit;
    using Moq;
    using SimpleAuth.Api.PermissionController;
    using SimpleAuth.Properties;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using Xunit;
    using Xunit.Abstractions;

    public class AddPermissionActionFixture
    {
        private readonly ITestOutputHelper _outputHelper;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<ITicketStore> _ticketStoreStub;
        private RuntimeSettings _configurationServiceStub;
        private RequestPermissionHandler _requestPermissionHandler;

        public AddPermissionActionFixture(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task When_RequiredParameter_ResourceSetId_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects(new ResourceSet { Id = Id.Create(), Name = "resource" });
            var addPermissionParameter = new PermissionRequest();

            var exception = await _requestPermissionHandler.Execute("tester", CancellationToken.None, addPermissionParameter)
                .ConfigureAwait(false) as Option<Ticket>.Error;
            Assert.Equal(ErrorCodes.InvalidRequest, exception.Details.Title);
            Assert.Equal(
                string.Format(
                    Strings.MissingParameter,
                    UmaConstants.AddPermissionNames.ResourceSetId),
                exception.Details.Detail);
        }

        [Fact]
        public async Task When_RequiredParameter_Scopes_Is_Not_Specified_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects(new ResourceSet { Id = Id.Create(), Name = "resource" });
            var addPermissionParameter = new PermissionRequest { ResourceSetId = "resource_set_id" };

            var exception =
                await _requestPermissionHandler.Execute("tester", CancellationToken.None, addPermissionParameter)
                    .ConfigureAwait(false) as Option<Ticket>.Error;
            Assert.Equal(ErrorCodes.InvalidRequest, exception.Details.Title);
            Assert.Equal(
                string.Format(Strings.MissingParameter, UmaConstants.AddPermissionNames.Scopes),
                exception.Details.Detail);
        }

        [Fact]
        public async Task When_ResourceSet_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects(new ResourceSet { Id = Id.Create(), Name = "resource" });
            var addPermissionParameter =
                new PermissionRequest { ResourceSetId = resourceSetId, Scopes = new[] { "scope" } };

            var exception =
                await _requestPermissionHandler.Execute("tester", CancellationToken.None, addPermissionParameter)
                    .ConfigureAwait(false) as Option<Ticket>.Error;
            Assert.Equal(ErrorCodes.InvalidResourceSetId, exception.Details.Title);
            Assert.Equal(string.Format(Strings.TheResourceSetDoesntExist, resourceSetId), exception.Details.Detail);
        }

        [Fact]
        public async Task When_Scope_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string resourceSetId = "resource_set_id";
            var addPermissionParameter = new PermissionRequest
            {
                ResourceSetId = resourceSetId,
                Scopes = new[] { ErrorCodes.InvalidScope }
            };
            var resources = new[] { new ResourceSet { Id = resourceSetId, Scopes = new[] { "scope" } } };
            InitializeFakeObjects(resources);

            var exception =
                await _requestPermissionHandler.Execute("tester", CancellationToken.None, addPermissionParameter)
                    .ConfigureAwait(false) as Option<Ticket>.Error;
            Assert.Equal(ErrorCodes.InvalidScope, exception.Details.Title);
            Assert.Equal(Strings.TheScopeAreNotValid, exception.Details.Detail);
        }

        [Fact]
        public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
        {
            var handler = new JwtSecurityTokenHandler();
            var idtoken = handler.CreateEncodedJwt(
                "test",
                "test",
                new ClaimsIdentity(new[] { new Claim("sub", "tester") }),
                null,
                null,
                null,
                null);
            const string resourceSetId = "resource_set_id";
            var addPermissionParameter = new PermissionRequest
            {
                ResourceSetId = resourceSetId,
                Scopes = new[] { "scope" },
                IdToken = idtoken
            };
            var resources = new[] { new ResourceSet { Id = resourceSetId, Scopes = new[] { "scope" } } };
            InitializeFakeObjects(resources);
            _ticketStoreStub.Setup(r => r.Add(It.IsAny<Ticket>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var ticket = (await _requestPermissionHandler
                .Execute("tester", CancellationToken.None, addPermissionParameter)
                .ConfigureAwait(false) as Option<Ticket>.Result)!.Item;

            Assert.NotEmpty(ticket.Requester);
        }

        private void InitializeFakeObjects(params ResourceSet[] resourceSets)
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetRepositoryStub
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, CancellationToken>(
                    (o, s, c) => Task.FromResult(resourceSets.FirstOrDefault(x => x.Id == s)));
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(resourceSets);
            _ticketStoreStub = new Mock<ITicketStore>();
            _configurationServiceStub = new RuntimeSettings(ticketLifeTime: TimeSpan.FromSeconds(2));
            _requestPermissionHandler = new RequestPermissionHandler(
                new InMemoryTokenStore(),
                _resourceSetRepositoryStub.Object,
                _configurationServiceStub,
                new TestOutputLogger("test", _outputHelper));
        }
    }
}
