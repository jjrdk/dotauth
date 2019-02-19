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

namespace SimpleAuth.Server.Tests.Policies
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth.Policies;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Responses;
    using Xunit;

    public class AuthorizationPolicyValidatorFixture
    {
        private readonly Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private readonly AuthorizationPolicyValidator _authorizationPolicyValidator;

        public AuthorizationPolicyValidatorFixture()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _authorizationPolicyValidator = new AuthorizationPolicyValidator(
                new Mock<IClientStore>().Object,
                _resourceSetRepositoryStub.Object,
                new Mock<IEventPublisher>().Object);
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => _authorizationPolicyValidator.IsAuthorized(null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _authorizationPolicyValidator.IsAuthorized(new Ticket(), null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceSet_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            var ticket = new Ticket {Lines = new [] {new TicketLine {ResourceSetId = "resource_set_id"}}};
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(() => Task.FromResult((ResourceSet) null));

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(
                    () => _authorizationPolicyValidator.IsAuthorized(ticket, "client_id", null, CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(ErrorDescriptions.SomeResourcesDontExist, exception.Message);
        }

        [Fact]
        public async Task When_Policy_Does_Not_Exist_Then_Authorized_Is_Returned()
        {
            var ticket = new Ticket {Lines = new [] {new TicketLine {ResourceSetId = "1"}}};
            var resourceSet = new [] {new ResourceSet {Id = "1"}};
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string[]>()))
                .ReturnsAsync(resourceSet);

            var result = await _authorizationPolicyValidator.IsAuthorized(ticket, "client_id", null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultEnum.Authorized, result.Type);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Is_Correct_Then_Authorized_Is_Returned()
        {
            var ticket = new Ticket {Lines = new [] {new TicketLine {ResourceSetId = "1"}}};
            var resourceSet = new []
            {
                new ResourceSet
                {
                    Id = "1",
                    AuthorizationPolicyIds = new [] {"authorization_policy_id"},
                    Policies = new [] {new Policy()}
                }
            };
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string[]>()))
                .ReturnsAsync(resourceSet);

            var result = await _authorizationPolicyValidator.IsAuthorized(ticket, "client_id", null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultEnum.Authorized, result.Type);
        }
    }
}
