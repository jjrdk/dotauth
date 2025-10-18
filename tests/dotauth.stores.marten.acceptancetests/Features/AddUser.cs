namespace DotAuth.Stores.Marten.AcceptanceTests.Features;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.User;
using NSubstitute;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private bool _userModified;
    private AddUserOperation _addUserOperation = null!;
    private RuntimeSettings _runtimeSettings = null!;
    private readonly IResourceOwnerRepository _resourceOwnerRepository = Substitute.For<IResourceOwnerRepository>();
    private readonly ISubjectBuilder _subjectBuilder = new DefaultSubjectBuilder();
    private readonly IEventPublisher _eventPublisher = Substitute.For<IEventPublisher>();
    private string _subject = null!;

    [Given(@"a configured resource owner repository")]
    public void GivenAConfiguredResourceOwnerRepository()
    {
        _resourceOwnerRepository
            .Insert(Arg.Any<ResourceOwner>(), Arg.Any<CancellationToken>())
            .Returns(true);
    }

    [Given(@"runtime settings")]
    public void GivenRuntimeSettings()
    {
        _runtimeSettings = new RuntimeSettings(string.Empty, _ => { _userModified = true; });
    }

    [Given(@"an AddUserOperation")]
    public void GivenAnAddUserOperation()
    {
        _addUserOperation = new AddUserOperation(
            _runtimeSettings,
            _resourceOwnerRepository,
            [],
            _subjectBuilder,
            _eventPublisher);
    }

    [When(@"local account user is added to storage")]
    public async Task WhenLocalAccountUserIsAddedToStorage()
    {
        var resourceOwner = new ResourceOwner { Subject = "tester", Password = "password", IsLocalAccount = true };
        var (_, s) = await _addUserOperation.Execute(resourceOwner, CancellationToken.None).ConfigureAwait(false);
        _subject = s;
    }

    [When(@"external account user is added to storage")]
    public async Task WhenExternalAccountUserIsAddedToStorage()
    {
        var resourceOwner = new ResourceOwner { Subject = "tester", IsLocalAccount = false };
        _ = await _addUserOperation.Execute(resourceOwner, CancellationToken.None).ConfigureAwait(false);
    }

    [Then(@"subject is not modified")]
    public void ThenSubjectIsNotModified()
    {
        Assert.Equal("tester", _subject);
    }

    [Then(@"user is modified")]
    public void ThenUserIsModified()
    {
        Assert.True(_userModified);
    }
}
