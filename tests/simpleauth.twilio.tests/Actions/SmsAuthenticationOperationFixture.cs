namespace SimpleAuth.Twilio.Tests.Actions
{
    using Moq;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Twilio;
    using Twilio.Actions;
    using Xunit;

    public class SmsAuthenticationOperationFixture
    {
        private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private readonly SmsAuthenticationOperation _smsAuthenticationOperation;

        public SmsAuthenticationOperationFixture()
        {
            var generateAndSendSmsCodeOperationStub = new Mock<IConfirmationCodeStore>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            var subjectBuilderStub = new Mock<ISubjectBuilder>();
            subjectBuilderStub.Setup(x => x.BuildSubject(It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(DateTime.UtcNow.Ticks.ToString);
            var smsAuthenticationOptions = new SmsAuthenticationOptions();
            _smsAuthenticationOperation = new SmsAuthenticationOperation(
                null,
                generateAndSendSmsCodeOperationStub.Object,
                _resourceOwnerRepositoryStub.Object,
                subjectBuilderStub.Object,
                new IAccountFilter[0],
                new Mock<IEventPublisher>().Object,
                smsAuthenticationOptions);
        }

        [Fact]
        public async Task When_Null_Parameter_Is_Passed_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _smsAuthenticationOperation.Execute(null, CancellationToken.None))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _smsAuthenticationOperation.Execute(string.Empty, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact(Skip = "Integration Test")]
        public async Task When_ResourceOwner_Exists_Then_ResourceOwner_Is_Returned()
        {
            const string phone = "phone";
            var resourceOwner = new ResourceOwner {Id = "id"};
            _resourceOwnerRepositoryStub
                .Setup(p => p.GetResourceOwnerByClaim("phone_number", phone, CancellationToken.None))
                .Returns(() => Task.FromResult(resourceOwner));

            var result = await _smsAuthenticationOperation.Execute(phone, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(resourceOwner.Id, result.Id);
        }
    }
}
