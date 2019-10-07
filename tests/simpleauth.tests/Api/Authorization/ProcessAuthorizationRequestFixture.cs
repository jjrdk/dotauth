namespace SimpleAuth.Tests.Api.Authorization
{
    using Moq;
    using Parameters;
    using Shared.Repositories;
    using SimpleAuth.Api.Authorization;
    using System;
    using SimpleAuth.Repositories;
    using Xunit;

    public sealed class ProcessAuthorizationRequestFixture
    {
        private ProcessAuthorizationRequest _processAuthorizationRequest;

        [Fact]
        public void When_Passing_NotValidRedirectUrl_To_AuthorizationParameter_Then_Exception_Is_Thrown()
        {
            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "not valid redirect url";
            Assert.Throws<UriFormatException>(
                () =>
                {
                    var authorizationParameter = new AuthorizationParameter
                    {
                        ClientId = clientId,
                        Prompt = "login",
                        State = state,
                        RedirectUrl = new Uri(redirectUrl)
                    };
                });
        }

        /*
        #region TEST VALID SCENARIOS

        [Fact]
        public void When_TryingToRequestAuthorization_But_TheUserConnectionValidityPeriodIsNotValid_Then_Redirect_To_The_Authentication_Screen()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            const long maxAge = 300;
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            currentDateTimeOffset -= maxAge + 100;
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                Prompt = "none",
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                MaxAge = 300
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString())
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

                        var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal);

                        Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(SimpleAuthEndPoints.AuthenticateIndex));
        }

        [Fact]
        public void When_TryingToRequestAuthorization_But_TheUserIsNotAuthenticated_Then_Redirect_To_The_Authentication_Screen()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
            };

                        var result = _processAuthorizationRequest.Process(authorizationParameter, null);

                        Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(Core.Results.SimpleAuthEndPoints.AuthenticateIndex));
        }

        [Fact]
        public void When_TryingToRequestAuthorization_And_TheUserIsAuthenticated_But_He_Didnt_Give_His_Consent_Then_Redirect_To_Consent_Screen()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
            };

            var claimIdentity = new ClaimsIdentity("fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

                        var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal);

                        Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(Core.Results.SimpleAuthEndPoints.ConsentIndex));
        }

        [Fact]
        public void When_TryingToRequestAuthorization_And_ExplicitySpecify_PromptConsent_But_The_User_IsNotAuthenticated_Then_Redirect_To_Consent_Screen()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                Prompt = "consent"
            };

                        var result = _processAuthorizationRequest.Process(authorizationParameter, null);

                        Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(Core.Results.SimpleAuthEndPoints.AuthenticateIndex));
        }

        [Fact]
        public void When_TryingToRequestAuthorization_And_TheUserIsAuthenticated_And_He_Already_Gave_HisConsent_Then_The_AuthorizationCode_Is_Passed_To_The_Callback()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string subject = "john.doe@email.com";
            const string redirectUrl = "http://localhost";
            FakeFactories.FakeDataSource.Consents.Add(new Consent
            {
                ResourceOwner = new ResourceOwner
                {
                    ClientId = subject
                },
                GrantedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                },
                Client = FakeFactories.FakeDataSource.Clients.First()
            });
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                Prompt = "none"
            };

            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.OpenIdClaimTypes.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

                        var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal);

                        Assert.NotNull(result);
            Assert.True(result.Type.Equals(ActionResultType.RedirectToCallBackUrl));
            Assert.True(result.RedirectInstruction.Parameters.Count().Equals(0));
        }

        #endregion

        #region TEST THE LOGIN

        [Fact]
        public void When_Executing_Correct_Authorization_Request_Then_Events_Are_Logged()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            const long maxAge = 300;
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            currentDateTimeOffset -= maxAge + 100;
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                Prompt = "none",
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code",
                MaxAge = 300
            };

            var jsonAuthorizationParameter = authorizationParameter.SerializeWithJavascript();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString())
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

                        var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal);

                        Assert.NotNull(result);
            Assert.True(result.RedirectInstruction.Action.Equals(SimpleAuthEndPoints.AuthenticateIndex));
            _simpleIdentityServerEventSource.Verify(s => s.StartProcessingAuthorizationRequest(jsonAuthorizationParameter));
            _simpleIdentityServerEventSource.Verify(s => s.EndProcessingAuthorizationRequest(jsonAuthorizationParameter, "RedirectToAction", "AuthenticateIndex"));
        }

        #endregion
        */
        private void InitializeMockingObjects()
        {
            var clientStore = new Mock<IClientStore>();
            var consentRepository = new Mock<IConsentRepository>();

            _processAuthorizationRequest = new ProcessAuthorizationRequest(
                clientStore.Object,
                consentRepository.Object,
                new InMemoryJwksRepository());
        }
    }
}
