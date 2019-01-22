namespace SimpleAuth.Tests.Helpers
{
    using Moq;
    using Parameters;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class ConsentHelperFixture
    {
        private Mock<IConsentRepository> _consentRepositoryFake;

        public ConsentHelperFixture()
        {
            InitializeFakeObjects();
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => _consentRepositoryFake.Object.GetConfirmedConsents("subject", null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_By_The_Resource_Owner_Then_Null_Is_Returned()
        {
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter();

            _consentRepositoryFake.Setup(c => c.GetConsentsForGivenUser(It.IsAny<string>()))
                .Returns(() => Task.FromResult((IEnumerable<Consent>)null));

            var result = await _consentRepositoryFake.Object.GetConfirmedConsents(subject, authorizationParameter)
                .ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task When_A_Consent_Has_Been_Given_For_Claim_Name_Then_Consent_Is_Returned()
        {
            const string subject = "subject";
            const string claimName = "name";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter
            {
                Claims = new ClaimsParameter
                {
                    UserInfo = new List<ClaimParameter> { new ClaimParameter { Name = claimName } }
                },
                ClientId = clientId
            };
            IEnumerable<Consent> consents = new List<Consent>
            {
                new Consent {Claims = new List<string> {claimName}, Client = new Client {ClientId = clientId}}
            };

            _consentRepositoryFake.Setup(c => c.GetConsentsForGivenUser(It.IsAny<string>()))
                .Returns(Task.FromResult(consents));

            var result = await _consentRepositoryFake.Object.GetConfirmedConsents(subject, authorizationParameter)
                .ConfigureAwait(false);

            Assert.Single(result.Claims);
            Assert.Equal(claimName, result.Claims.First());
        }

        [Fact]
        public async Task When_A_Consent_Has_Been_Given_For_Scope_Profile_Then_Consent_Is_Returned()
        {
            const string subject = "subject";
            const string scope = "profile";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter { ClientId = clientId, Scope = scope };
            IEnumerable<Consent> consents = new List<Consent>
            {
                new Consent
                {
                    Client = new Client {ClientId = clientId},
                    GrantedScopes = new List<Scope> {new Scope {Name = scope}}
                }
            };
            var scopes = new List<string> { scope };

            _consentRepositoryFake.Setup(c => c.GetConsentsForGivenUser(It.IsAny<string>()))
                .Returns(Task.FromResult(consents));

            var result = await _consentRepositoryFake.Object.GetConfirmedConsents(subject, authorizationParameter)
                .ConfigureAwait(false);

            Assert.Single(result.GrantedScopes);
            Assert.Equal(scope, result.GrantedScopes.First().Name);
        }

        [Fact]
        public async Task
            When_Consent_Has_Been_Assigned_To_OpenId_Profile_And_Request_Consent_For_Scope_OpenId_Profile_Email_Then_Null_Is_Returned()
        {
            const string subject = "subject";
            const string openIdScope = "openid";
            const string profileScope = "profile";
            const string emailScope = "email";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = openIdScope + " " + profileScope + " " + emailScope
            };
            IEnumerable<Consent> consents = new List<Consent>
            {
                new Consent
                {
                    Client = new Client {ClientId = clientId},
                    GrantedScopes = new List<Scope>
                    {
                        new Scope {Name = profileScope}, new Scope {Name = openIdScope}
                    }
                }
            };

            //var scopes = new List<string> {openIdScope, profileScope, emailScope};

            _consentRepositoryFake.Setup(c => c.GetConsentsForGivenUser(It.IsAny<string>()))
                .Returns(Task.FromResult(consents));

            var result = await _consentRepositoryFake.Object.GetConfirmedConsents(subject, authorizationParameter)
                .ConfigureAwait(false);

            Assert.Null(result);
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryFake = new Mock<IConsentRepository>();
        }
    }
}
