namespace SimpleAuth.Twilio.Tests.Actions
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth.Services;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Twilio;
    using Twilio.Actions;
    using WebSite.User.Actions;
    using Xunit;

    public class SmsAuthenticationOperationFixture
    {
        private Mock<IGenerateAndSendSmsCodeOperation> _generateAndSendSmsCodeOperationStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IAddUserOperation> _userActionsStub;
        private Mock<ISubjectBuilder> _subjectBuilderStub;
        private SmsAuthenticationOptions _smsAuthenticationOptions;
        private ISmsAuthenticationOperation _smsAuthenticationOperation;

        [Fact]
        public async Task When_Null_Parameter_Is_Passed_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _smsAuthenticationOperation.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _smsAuthenticationOperation.Execute(string.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_Exists_Then_ResourceOwner_Is_Returned()
        {            const string phone = "phone";
            var resourceOwner = new ResourceOwner
            {
                Id = "id"
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(p => p.GetResourceOwnerByClaim("phone_number", phone)).Returns(() => Task.FromResult(resourceOwner));

                        var result = await _smsAuthenticationOperation.Execute(phone).ConfigureAwait(false);

                        _generateAndSendSmsCodeOperationStub.Verify(s => s.Execute(phone));
            Assert.NotNull(result);
            Assert.Equal(resourceOwner.Id, result.Id);
        }

        [Fact]
        public async Task When_AutomaticScimResourceCreation_Is_Enabled_Then_Operation_Is_Called()
        {            const string phone = "phone";

            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(p => p.GetResourceOwnerByClaim("phone", phone)).Returns(() => Task.FromResult((ResourceOwner)null));
            _smsAuthenticationOptions.AuthorizationWellKnownConfiguration = "auth";
            _smsAuthenticationOptions.ClientId = "clientid";
            _smsAuthenticationOptions.ClientSecret = "clientsecret";
            _smsAuthenticationOptions.ScimBaseUrl = new Uri("https://scim");

                        await _smsAuthenticationOperation.Execute(phone).ConfigureAwait(false);

                        _generateAndSendSmsCodeOperationStub.Verify(s => s.Execute(phone));
            _userActionsStub.Verify(u => u.Execute(It.IsAny<ResourceOwner>(), It.IsAny<Uri>()));
        }

        [Fact]
        public async Task When_AutomaticScimResourceCreation_Is_Not_Enabled_Then_Operation_Is_Called()
        {            const string phone = "phone";

            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(p => p.GetResourceOwnerByClaim("phone", phone)).Returns(() => Task.FromResult((ResourceOwner)null));
            _smsAuthenticationOptions.ScimBaseUrl = null;
            //_smsAuthenticationOptions.AuthenticationOptions = new Basic.BasicAuthenticationOptions();

                        await _smsAuthenticationOperation.Execute(phone).ConfigureAwait(false);

                        _generateAndSendSmsCodeOperationStub.Verify(s => s.Execute(phone));
            _userActionsStub.Verify(u => u.Execute(It.IsAny<ResourceOwner>(), null));
        }

        private void InitializeFakeObjects()
        {
            _generateAndSendSmsCodeOperationStub = new Mock<IGenerateAndSendSmsCodeOperation>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _subjectBuilderStub = new Mock<ISubjectBuilder>();
            _userActionsStub = new Mock<IAddUserOperation>();
            _smsAuthenticationOptions = new SmsAuthenticationOptions();
            _smsAuthenticationOperation = new SmsAuthenticationOperation(
                _generateAndSendSmsCodeOperationStub.Object,
                _resourceOwnerRepositoryStub.Object, _userActionsStub.Object, _subjectBuilderStub.Object, _smsAuthenticationOptions);
        }
    }
}
