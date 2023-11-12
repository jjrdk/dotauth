namespace DotAuth.Tests.Api.Sms.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Sms.Services;
using NSubstitute;
using Xunit;

public sealed class SmsAuthenticateResourceOwnerServiceFixture
{
    private readonly IResourceOwnerRepository _resourceOwnerRepositoryStub;
    private readonly IConfirmationCodeStore _confirmationCodeStoreStub;
    private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

    public SmsAuthenticateResourceOwnerServiceFixture()
    {
        _resourceOwnerRepositoryStub = Substitute.For<IResourceOwnerRepository>();
        _confirmationCodeStoreStub = Substitute.For<IConfirmationCodeStore>();
        _authenticateResourceOwnerService = new SmsAuthenticateResourceOwnerService(
            _resourceOwnerRepositoryStub,
            _confirmationCodeStoreStub);
    }

    [Fact]
    public async Task When_ConfirmationCode_Does_Not_Exist_Then_Null_Is_Returned()
    {
        _confirmationCodeStoreStub.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ConfirmationCode?)null);

        var result = await _authenticateResourceOwnerService
                .AuthenticateResourceOwner("login", "password", CancellationToken.None)
            ;

        Assert.Null(result);
    }

    [Fact]
    public async Task When_Subject_Is_Different_Then_Null_Is_Returned()
    {
        _confirmationCodeStoreStub.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ConfirmationCode { Subject = "sub" });

        var result = await _authenticateResourceOwnerService
            .AuthenticateResourceOwner("login", "password", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task When_ConfirmationCode_Is_Expired_Then_Null_Is_Returned()
    {
        const string login = "login";
        _confirmationCodeStoreStub.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ConfirmationCode
            {
                Subject = login, IssueAt = DateTimeOffset.UtcNow.AddDays(-1), ExpiresIn = 100
            });

        var result = await _authenticateResourceOwnerService
            .AuthenticateResourceOwner(login, "password", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task When_ConfirmationCode_Is_Correct_And_PhoneNumber_Correct_Then_Operation_Is_Called()
    {
        const string login = "login";
        _confirmationCodeStoreStub.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ConfirmationCode { Subject = login, IssueAt = DateTimeOffset.UtcNow, ExpiresIn = 100 });
        _resourceOwnerRepositoryStub.GetResourceOwnerByClaim(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new ResourceOwner { Subject = login });

        await _authenticateResourceOwnerService.AuthenticateResourceOwner(login, "password", CancellationToken.None)
            ;

        await _resourceOwnerRepositoryStub.Received().GetResourceOwnerByClaim(
            OpenIdClaimTypes.PhoneNumber,
            login,
            CancellationToken.None);
        await _confirmationCodeStoreStub.Received().Remove("password", login, Arg.Any<CancellationToken>());
    }
}
