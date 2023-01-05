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

namespace DotAuth.Server.Tests.Policies;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Parameters;
using DotAuth.Policies;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Logging;
using Moq;
using Xunit;

public sealed class AuthorizationPolicyValidatorFixture
{
    private readonly Mock<IResourceSetRepository> _resourceSetRepositoryStub;
    private readonly AuthorizationPolicyValidator _authorizationPolicyValidator;
    private readonly Mock<IClientStore> _clientStoreStub;
    private readonly InMemoryJwksRepository _inMemoryJwksRepository;

    public AuthorizationPolicyValidatorFixture()
    {
        IdentityModelEventSource.ShowPII = true;
        _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
        _clientStoreStub = new Mock<IClientStore>();
        _inMemoryJwksRepository = new InMemoryJwksRepository();
        _authorizationPolicyValidator = new AuthorizationPolicyValidator(
            _inMemoryJwksRepository,
            _resourceSetRepositoryStub.Object,
            new Mock<IEventPublisher>().Object);
    }

    [Fact]
    public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
    {
        await Assert.ThrowsAsync<NullReferenceException>(
                () => _authorizationPolicyValidator.IsAuthorized(null, null, null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task WhenPassingEmptyTicketParameterThenExceptionsAreThrown()
    {
        await Assert.ThrowsAsync<ArgumentException>(
                () => _authorizationPolicyValidator.IsAuthorized(new Ticket(), null, null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task WhenResourceSetDoesNotExistThenReturnsNotAuthorized()
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonWebKey = await _inMemoryJwksRepository.GetDefaultSigningKey().ConfigureAwait(false);
        var token = handler.CreateEncodedJwt("test", "test", new ClaimsIdentity(), null, null, null, jsonWebKey);
        var ticket = new Ticket { Lines = new[] { new TicketLine { ResourceSetId = "resource_set_id" } } };
        _resourceSetRepositoryStub
            .Setup(r => r.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult((ResourceSet)null));

        var result = await _authorizationPolicyValidator.IsAuthorized(
                ticket,
                new Client { ClientId = "client_id" },
                new ClaimTokenParameter { Format = UmaConstants.IdTokenType, Token = token },
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(AuthorizationPolicyResultKind.NotAuthorized, result.Result);
    }

    [Fact]
    public async Task When_Policy_Does_Not_Exist_Then_RequestSubmitted_Is_Returned()
    {
        var handler = new JwtSecurityTokenHandler();
        var key = await _inMemoryJwksRepository.GetDefaultSigningKey().ConfigureAwait(false);

        var token = handler.CreateEncodedJwt(
            "test",
            "test",
            new ClaimsIdentity(new[] { new Claim("test", "test") }),
            null,
            DateTime.UtcNow.AddYears(1),
            DateTime.UtcNow,
            key);
        var ticket = new Ticket { Lines = new[] { new TicketLine { ResourceSetId = "1" } } };
        var resourceSet = new[] { new ResourceSet { Id = "1" } };
        _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(resourceSet);

        var result = await _authorizationPolicyValidator.IsAuthorized(
                ticket,
                new Client { ClientId = "client_id" },
                new ClaimTokenParameter { Format = "access_token", Token = token },
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(AuthorizationPolicyResultKind.RequestSubmitted, result.Result);
    }

    [Fact]
    public async Task When_AuthorizationPolicy_Is_Correct_Then_Authorized_Is_Returned()
    {
        var handler = new JwtSecurityTokenHandler();
        var key = await _inMemoryJwksRepository.GetDefaultSigningKey().ConfigureAwait(false);

        var token = handler.CreateEncodedJwt(
            "test",
            "test",
            new ClaimsIdentity(new[] { new Claim("test", "test") }),
            null,
            DateTime.UtcNow.AddYears(1),
            DateTime.UtcNow,
            key);
        _clientStoreStub.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((s, c) => Task.FromResult(new Client { ClientId = s }));

        var ticket = new Ticket { Lines = new[] { new TicketLine { ResourceSetId = "1", Scopes = new[] { "read" } } } };
        var resourceSet = new[]
        {
            new ResourceSet
            {
                Id = "1",
                AuthorizationPolicies = new[]
                {
                    new PolicyRule
                    {
                        Scopes = new[] { "read" },
                        ClientIdsAllowed = new[] { "client_id" },
                        Claims = new[] { new ClaimData { Type = "test", Value = "test" } }
                    }
                }
            }
        };
        _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(resourceSet);

        var result = await _authorizationPolicyValidator.IsAuthorized(
                ticket,
                new Client { ClientId = "client_id" },
                new ClaimTokenParameter { Token = token, Format = UmaConstants.IdTokenType },
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(AuthorizationPolicyResultKind.Authorized, result.Result);
    }
}
