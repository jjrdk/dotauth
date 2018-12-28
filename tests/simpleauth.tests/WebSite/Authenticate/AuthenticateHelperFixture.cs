namespace SimpleAuth.Tests.WebSite.Authenticate
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Errors;
    using Factories;
    using Moq;
    using Parameters;
    using Results;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Common;
    using SimpleAuth.Helpers;
    using SimpleAuth.WebSite.Authenticate.Common;
    using Xunit;

    public sealed class AuthenticateHelperFixture
    {
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IActionResultFactory> _actionResultFactoryFake;
        private Mock<IConsentHelper> _consentHelperFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IClientStore> _clientRepositoryStub;
        private IAuthenticateHelper _authenticateHelper;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateHelper.ProcessRedirection(null, null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult((Client)null));
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = "client_id"
            };

                        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _authenticateHelper.ProcessRedirection(authorizationParameter, null, null, null, null)).ConfigureAwait(false);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientIdDoesntExist, authorizationParameter.ClientId));
        }

        [Fact]
        public async Task When_PromptConsent_Parameter_Is_Passed_Then_Redirect_To_ConsentScreen()
        {            InitializeFakeObjects();
            const string subject = "subject";
            const string code = "code";
            var prompts = new List<PromptParameter>
            {
                PromptParameter.consent
            };
            var actionResult = new EndpointResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>();
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirection())
                .Returns(actionResult);
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(prompts);

                        actionResult = await _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                subject,
                claims, null).ConfigureAwait(false);

                        Assert.True(actionResult.RedirectInstruction.Action == IdentityServerEndPoints.ConsentIndex);
            Assert.Contains(actionResult.RedirectInstruction.Parameters, p => p.Name == code && p.Value == code);
        }

        [Fact]
        public async Task When_Consent_Has_Already_Been_Given_Then_Redirect_To_Callback()
        {            InitializeFakeObjects();
            const string subject = "subject";
            const string code = "code";
            var prompts = new List<PromptParameter>
            {
                PromptParameter.none
            };
            var actionResult = new EndpointResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var consent = new Consent();
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseMode = ResponseMode.form_post
            };
            var claims = new List<Claim>();
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(prompts);
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(consent));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);

                        actionResult = await _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                subject,
                claims, null).ConfigureAwait(false);

                        Assert.True(actionResult.RedirectInstruction.ResponseMode == ResponseMode.form_post);
        }

        [Fact]
        public async Task When_There_Is_No_Consent_Then_Redirect_To_Consent_Screen()
        {            InitializeFakeObjects();
            const string subject = "subject";
            const string code = "code";
            var prompts = new List<PromptParameter>
            {
                PromptParameter.none
            };
            var actionResult = new EndpointResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>();
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirection())
                .Returns(actionResult);
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(prompts);
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>())).Returns(() => Task.FromResult((Consent)null));

                        actionResult = await _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                subject,
                claims, null).ConfigureAwait(false);

                        Assert.True(actionResult.RedirectInstruction.Action == IdentityServerEndPoints.ConsentIndex);
            Assert.Contains(actionResult.RedirectInstruction.Parameters, p => p.Name == code && p.Value == code);
        }

        private void InitializeFakeObjects()
        {
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _clientRepositoryStub = new Mock<IClientStore>();
            _authenticateHelper = new AuthenticateHelper(
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object,
                _consentHelperFake.Object,
                _generateAuthorizationResponseFake.Object,
                _clientRepositoryStub.Object);
        }
    }
}
