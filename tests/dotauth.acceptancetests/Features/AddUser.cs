namespace DotAuth.AcceptanceTests.Features;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.User;
using Moq;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private bool _userModified;
    private AddUserOperation? _addUserOperation;
    private RuntimeSettings? _runtimeSettings;
    private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepository = new();
    private readonly ISubjectBuilder _subjectBuilder = new DefaultSubjectBuilder();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private string _subject = null!;
    
    [Given(@"a configured resource owner repository")]
    public void GivenAConfiguredResourceOwnerRepository()
    {
        _resourceOwnerRepository
            .Setup(x => x.Insert(It.IsAny<ResourceOwner>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
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
            _runtimeSettings!,
            _resourceOwnerRepository.Object,
            System.Array.Empty<IAccountFilter>(),
            _subjectBuilder,
            _eventPublisher.Object);
    }

    [When(@"local account user is added to storage")]
    public async Task WhenLocalAccountUserIsAddedToStorage()
    {
        var resourceOwner = new ResourceOwner { Subject = "tester", Password = "password", IsLocalAccount = true };
        var (_, s) = await _addUserOperation!.Execute(resourceOwner, CancellationToken.None).ConfigureAwait(false);
        _subject = s;
    }

    [When(@"external account user is added to storage")]
    public async Task WhenExternalAccountUserIsAddedToStorage()
    {
        var resourceOwner = new ResourceOwner { Subject = "tester", IsLocalAccount = false };
        _ = await _addUserOperation!.Execute(resourceOwner, CancellationToken.None).ConfigureAwait(false);
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