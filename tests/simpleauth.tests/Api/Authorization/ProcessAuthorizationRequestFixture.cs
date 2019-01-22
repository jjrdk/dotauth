namespace SimpleAuth.Tests.Api.Authorization
{
    using Moq;
    using Parameters;
    using Shared.Repositories;
    using SimpleAuth.Api.Authorization;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class ProcessAuthorizationRequestFixture
    {
        private ProcessAuthorizationRequest _processAuthorizationRequest;

        [Fact(Skip = "Invalid code path")]
        public async Task When_Passing_NullAuthorization_To_Function_Then_ArgumentNullException_Is_Thrown()
        {
            InitializeMockingObjects();

            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    _processAuthorizationRequest.ProcessAsync(null, null, null, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _processAuthorizationRequest.ProcessAsync(new AuthorizationParameter(), null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public void When_Passing_NotValidRedirectUrl_To_AuthorizationParameter_Then_Exception_Is_Thrown()
        {
            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "not valid redirect url";
            Assert.Throws<UriFormatException>(() =>
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
        [Fact]
        public async Task When_Passing_AuthorizationParameterWithoutOpenIdScope_Then_Exception_Is_Thrown()
        {
            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "email"
            };

            var exception =await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
        () => _processAuthorizationRequest.ProcessAsync(authorizationParameter, null));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidScope));
            Assert.True(exception.Message.Equals(string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, CoreConstants.StandardScopes.OpenId.Name)));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_Passing_AuthorizationRequestWithMissingResponseType_Then_Exception_Is_Thrown()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid"
            };

                        var exception =
                Assert.Throws<SimpleAuthExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(string.Format(ErrorDescriptions.MissingParameter
                , Core.JwtConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName)));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_Passing_AuthorizationRequestWithNotSupportedResponseType_Then_Exception_Is_Thrown()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "login",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code"
            };

                        var client = FakeFactories.FakeDataSource.Clients.FirstOrDefault(c => c.ClientId == clientId);
            Assert.NotNull(client);
            client.ResponseTypes.Remove(ResponseType.code);

                        var exception =
                Assert.Throws<SimpleAuthExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType
                , clientId, "code")));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_TryingToByPassLoginAndConsentScreen_But_UserIsNotAuthenticated_Then_Exception_Is_Thrown()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "none",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code"
            };

                        var exception =
                Assert.Throws<SimpleAuthExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, null));
            Assert.True(exception.Code.Equals(ErrorCodes.LoginRequiredCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheUserNeedsToBeAuthenticated));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_TryingToByPassLoginAndConsentScreen_But_TheUserDidntGiveHisConsent_Then_Exception_Is_Thrown()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "none",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code"
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));

                        var exception =
                Assert.Throws<SimpleAuthExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InteractionRequiredCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheUserNeedsToGiveHisConsent));
            Assert.True(exception.State.Equals(state));
        }

        [Fact]
        public void When_Passing_A_NotValid_IdentityTokenHint_Parameter_Then_An_Exception_Is_Thrown()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string subject = "john.doe@email.com";
            const string redirectUrl = "http://localhost";
            FakeFactories.FakeDataSource.Consents.Add(new Consent
            {
                ResourceOwner = new ResourceOwner
                {
                    Id = subject
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
                Prompt = "none",
                IdTokenHint = "invalid identity token hint"
            };

            var claims = new List<Claim>
            {
                new Claim(Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

                        var exception = Assert.Throws<SimpleAuthExceptionWithState>(() => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheIdTokenHintParameterIsNotAValidToken));
        }

        [Fact]
        public void When_Passing_An_IdentityToken_Valid_ForWrongAudience_Then_An_Exception_Is_Thrown()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string subject = "john.doe@email.com";
            const string redirectUrl = "http://localhost";
            FakeFactories.FakeDataSource.Consents.Add(new Consent
            {
                ResourceOwner = new ResourceOwner
                {
                    Id = subject
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
                Prompt = "none",
            };

            var subjectClaim = new Claim(Core.Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject, subject);
            var claims = new List<Claim>
            {
                subjectClaim
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var jwtPayload = new JwtSecurityToken
            {
                {
                    subjectClaim.Type, subjectClaim.Value
                }
            };

            authorizationParameter.IdTokenHint = _jwtGenerator.Sign(jwtPayload, JwsAlg.RS256);

                        var exception = Assert.Throws<SimpleAuthExceptionWithState>(() => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheIdentityTokenDoesntContainSimpleAuthAsAudience));
        }
        
        [Fact]
        public void When_Passing_An_IdentityToken_Different_From_The_Current_Authenticated_User_Then_An_Exception_Is_Thrown()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string subject = "john.doe@email.com";
            const string issuerName = "audience";
            const string redirectUrl = "http://localhost";
            FakeFactories.FakeDataSource.Consents.Add(new Consent
            {
                ResourceOwner = new ResourceOwner
                {
                    Id = subject
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
                Prompt = "none",
            };

            var subjectClaim = new Claim(Core.Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject, subject);
            var claims = new List<Claim>
            {
                subjectClaim
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var jwtPayload = new JwtSecurityToken
            {
                {
                    subjectClaim.Type, "wrong subjet"
                },
                {
                    Jwt.JwtConstants.StandardClaimNames.Audiences, new [] {  issuerName }
                }
            };
            _oAuthConfiguration.Setup(s => s.GetIssuerName()).Returns(issuerName);
            authorizationParameter.IdTokenHint = _jwtGenerator.Sign(jwtPayload, JwsAlg.RS256);

                        var exception = Assert.Throws<SimpleAuthExceptionWithState>(() => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(ErrorDescriptions.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken));
        }

        [Fact]
        public void When_Passing_Not_Supported_Prompts_Parameter_Then_An_Exception_Is_Thrown()
        {            InitializeMockingObjects();
            const string state = "state";
            const string clientId = "MyBlog";
            const string redirectUrl = "http://localhost";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Prompt = "select_account",
                State = state,
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = "code"
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));

                        var exception =
                Assert.Throws<SimpleAuthExceptionWithState>(
                    () => _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal));
            Assert.True(exception.Code.Equals(ErrorCodes.InvalidRequestCode));
            Assert.True(exception.Message.Equals(string.Format(ErrorDescriptions.ThePromptParameterIsNotSupported, "select_account")));
            Assert.True(exception.State.Equals(state));
        }
        */
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
                    Id = subject
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
                new Claim(Core.Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

                        var result = _processAuthorizationRequest.Process(authorizationParameter, claimsPrincipal);
            
                        Assert.NotNull(result);
            Assert.True(result.Type.Equals(TypeActionResult.RedirectToCallBackUrl));
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
                consentRepository.Object);
        }
    }
}
