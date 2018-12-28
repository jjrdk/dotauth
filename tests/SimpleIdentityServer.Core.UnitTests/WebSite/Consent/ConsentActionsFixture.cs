namespace SimpleAuth.Tests.WebSite.Consent
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using Parameters;
    using SimpleAuth.WebSite.Consent;
    using SimpleAuth.WebSite.Consent.Actions;
    using Xunit;

    public sealed class ConsentActionsFixture
    {
        private Mock<IDisplayConsentAction> _displayConsentActionFake;
        private Mock<IConfirmConsentAction> _confirmConsentActionFake;
        private IConsentActions _consentActions;

        [Fact]
        public async Task When_Passing_Null_Parameter_To_DisplayConsent_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

                        await Assert.ThrowsAsync<ArgumentNullException>(
                () => _consentActions.DisplayConsent(null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _consentActions.DisplayConsent(authorizationParameter, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_ConfirmConsent_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

                        await Assert.ThrowsAsync<ArgumentNullException>(
                () => _consentActions.ConfirmConsent(null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _consentActions.ConfirmConsent(authorizationParameter, null, null)).ConfigureAwait(false);
        }

        private void InitializeFakeObjects()
        {
            _displayConsentActionFake = new Mock<IDisplayConsentAction>();
            _confirmConsentActionFake = new Mock<IConfirmConsentAction>();
            _consentActions = new ConsentActions(_displayConsentActionFake.Object,
                _confirmConsentActionFake.Object);
        }
    }
}
