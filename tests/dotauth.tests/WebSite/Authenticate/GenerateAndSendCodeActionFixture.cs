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


namespace DotAuth.Tests.WebSite.Authenticate;

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth.Exceptions;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.Authenticate;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public sealed class GenerateAndSendCodeActionFixture
{
    private readonly IResourceOwnerRepository _resourceOwnerRepositoryStub;
    private readonly IConfirmationCodeStore _confirmationCodeStoreStub;
    private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandlerStub;
    private readonly GenerateAndSendCodeAction _generateAndSendCodeAction;

    public GenerateAndSendCodeActionFixture(ITestOutputHelper outputHelper)
    {
        _resourceOwnerRepositoryStub = Substitute.For<IResourceOwnerRepository>();
        _confirmationCodeStoreStub = Substitute.For<IConfirmationCodeStore>();
        _twoFactorAuthenticationHandlerStub = Substitute.For<ITwoFactorAuthenticationHandler>();
        _generateAndSendCodeAction = new GenerateAndSendCodeAction(
            _resourceOwnerRepositoryStub,
            _confirmationCodeStoreStub,
            _twoFactorAuthenticationHandlerStub,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task WhenPassingEmptyParameterThenErrorIsReturned()
    {
        var error = await _generateAndSendCodeAction.Send("", CancellationToken.None)
            .ConfigureAwait(false);

        Assert.IsType<Option<string>.Error>(error);
    }

    [Fact]
    public async Task When_ResourceOwner_Does_Not_Exist_Then_Exception_Is_Thrown()
    {
        _resourceOwnerRepositoryStub.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ResourceOwner)null);

        var exception = await _generateAndSendCodeAction.Send("subject", CancellationToken.None)
            .ConfigureAwait(false) as Option<string>.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Details.Title);
        Assert.Equal(Strings.TheRoDoesntExist, exception.Details.Detail);
    }

    [Fact]
    public async Task When_Two_Factor_Auth_Is_Not_Enabled_Then_Exception_Is_Thrown()
    {
        _resourceOwnerRepositoryStub.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceOwner { TwoFactorAuthentication = string.Empty });

        var exception = await _generateAndSendCodeAction.Send("subject", CancellationToken.None)
            .ConfigureAwait(false) as Option<string>.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Details.Title);
        Assert.Equal(Strings.TwoFactorAuthenticationIsNotEnabled, exception.Details.Detail);
    }

    [Fact]
    public async Task When_ResourceOwner_Does_Not_Have_The_Required_Claim_Then_Exception_Is_Thrown()
    {
        _resourceOwnerRepositoryStub.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(
                    new ResourceOwner
                    {
                        TwoFactorAuthentication = "email",
                        Claims = new[] { new Claim("key", "value") },
                        Subject = "subject"
                    }));
        var fakeAuthService = Substitute.For<ITwoFactorAuthenticationService>();
        fakeAuthService.RequiredClaim.Returns("claim");
        _twoFactorAuthenticationHandlerStub.Get(Arg.Any<string>()).Returns(fakeAuthService);

        await Assert
            .ThrowsAsync<ClaimRequiredException>(
                () => _generateAndSendCodeAction.Send("subject", CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_Code_Cannot_Be_Inserted_Then_Exception_Is_Thrown()
    {
        _resourceOwnerRepositoryStub.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(
                    new ResourceOwner
                    {
                        TwoFactorAuthentication = "email",
                        Claims = new[] { new Claim("key", "value") },
                        Subject = "subject"
                    }));
        var fakeAuthService = Substitute.For<ITwoFactorAuthenticationService>();
        fakeAuthService.RequiredClaim.Returns("key");
        _twoFactorAuthenticationHandlerStub.Get(Arg.Any<string>()).Returns(fakeAuthService);
        _confirmationCodeStoreStub
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ConfirmationCode)null);
        _confirmationCodeStoreStub.Add(Arg.Any<ConfirmationCode>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var exception = await _generateAndSendCodeAction.Send("subject", CancellationToken.None)
            .ConfigureAwait(false) as Option<string>.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Details.Title);
        Assert.Equal(Strings.TheConfirmationCodeCannotBeSaved, exception.Details.Detail);
    }

    [Fact]
    public async Task When_Code_Is_Generated_And_Inserted_Then_Handler_Is_Called()
    {
        _resourceOwnerRepositoryStub.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(
                    new ResourceOwner
                    {
                        TwoFactorAuthentication = "email",
                        Claims = new[] { new Claim("key", "value") },
                        Subject = "subject"
                    }));
        var fakeAuthService = Substitute.For<ITwoFactorAuthenticationService>();
        fakeAuthService.RequiredClaim.Returns("key");
        _twoFactorAuthenticationHandlerStub.Get(Arg.Any<string>()).Returns(fakeAuthService);
        _confirmationCodeStoreStub
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ConfirmationCode)null);
        _confirmationCodeStoreStub.Add(Arg.Any<ConfirmationCode>(), Arg.Any<CancellationToken>())
            .Returns(true);

        await _generateAndSendCodeAction.Send("subject", CancellationToken.None).ConfigureAwait(false);

        await _twoFactorAuthenticationHandlerStub.Received()
            .SendCode(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ResourceOwner>());
    }
}
