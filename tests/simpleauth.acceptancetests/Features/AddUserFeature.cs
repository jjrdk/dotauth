namespace SimpleAuth.AcceptanceTests.Features
{
    using Moq;
    using SimpleAuth.Services;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System.Threading;
    using SimpleAuth.Events;
    using SimpleAuth.WebSite.User;
    using Xbehave;
    using Xunit;

    public class AddUserFeature
    {
        private bool _userModified;
        private AddUserOperation _addUserOperation;
        private RuntimeSettings _runtimeSettings;
        private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepository = new Mock<IResourceOwnerRepository>();
        private readonly ISubjectBuilder _subjectBuilder = new DefaultSubjectBuilder();
        private readonly Mock<IEventPublisher> _eventPublisher = new Mock<IEventPublisher>();

        [Background]
        public void Background()
        {
            "Given a configured resource owner repository".x(
                () =>
                {
                    _resourceOwnerRepository
                        .Setup(x => x.Insert(It.IsAny<ResourceOwner>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);
                });

            "And runtime settings".x(() => { _runtimeSettings = new RuntimeSettings(r => { _userModified = true; }); });

            "And an AddUserOperation".x(() =>
                {
                    _addUserOperation = new AddUserOperation(
                        _runtimeSettings,
                        _resourceOwnerRepository.Object,
                        System.Array.Empty<IAccountFilter>(),
                        _subjectBuilder,
                        _eventPublisher.Object);
                });
        }

        [Scenario(DisplayName = "Local account subject not modified during creation")]
        public void LocalAccountSubjectNotModified()
        {
            string subject = null;

            "When local account user is added to storage".x(
                async () =>
                {
                    var resourceOwner = new ResourceOwner { Subject = "tester", Password = "password", IsLocalAccount = true };
                    var (_, s) = await _addUserOperation.Execute(resourceOwner, CancellationToken.None).ConfigureAwait(false);
                    subject = s;
                });

            "Then subject is not modified".x(
                () =>
                {
                    Assert.Equal("tester", subject);
                });
        }

        [Scenario(DisplayName = "Local user modification during creation")]
        public void LocalUserModificationDuringCreation()
        {
            "When local account user is added to storage".x(
                async () =>
                {
                    var resourceOwner = new ResourceOwner { Subject = "tester", IsLocalAccount = true };
                    _ = await _addUserOperation.Execute(resourceOwner, CancellationToken.None).ConfigureAwait(false);
                });

            "Then user is modified".x(
                () =>
                {
                    Assert.True(_userModified);
                });
        }

        [Scenario(DisplayName = "External user modification during creation")]
        public void ExternalUserModificationDuringCreation()
        {
            "When external account user is added to storage".x(
                async () =>
                {
                    var resourceOwner = new ResourceOwner { Subject = "tester", IsLocalAccount = false };
                    _ = await _addUserOperation.Execute(resourceOwner, CancellationToken.None).ConfigureAwait(false);
                });

            "Then user is modified".x(
                () =>
                {
                    Assert.True(_userModified);
                });
        }
    }
}
