// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Moq;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Introspection.Actions
{
    using SimpleAuth;
    using SimpleAuth.Api.Introspection.Actions;
    using SimpleAuth.Authenticate;
    using SimpleAuth.Errors;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Logging;
    using SimpleAuth.Parameters;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Validators;

    public class PostIntrospectionActionFixture
    {
        private Mock<IOAuthEventSource> _oauthEventSource;
        private Mock<IAuthenticateClient> _authenticateClientStub;
        private Mock<IIntrospectionParameterValidator> _introspectionParameterValidatorStub;
        private Mock<ITokenStore> _tokenStoreStub;
        private IPostIntrospectionAction _postIntrospectionAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _postIntrospectionAction.Execute(null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var parameter = new IntrospectionParameter
            {
                Token = "token"
            };
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
               .Returns(Task.FromResult(new AuthenticationResult(null, null)));

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _postIntrospectionAction.Execute(parameter, null, null)).ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidClient);
        }

        [Fact]
        public async Task When_AccessToken_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.AccessToken,
                Token = "token"
            };
            var client = new AuthenticationResult(new Client(), null);
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
                .Returns(Task.FromResult(client));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _postIntrospectionAction.Execute(parameter, null, null)).ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidToken);
            Assert.True(exception.Message == ErrorDescriptions.TheTokenIsNotValid);
        }

        [Fact]
        public async Task When_Passing_Expired_AccessToken_Then_Result_Should_Be_Returned()
        {            InitializeFakeObjects();
            const string clientId = "client_id";
            const string subject = "subject";
            const string audience = "audience";
            var authenticationHeaderValue = new AuthenticationHeaderValue("Basic", "ClientId:ClientSecret".Base64Encode());
            var audiences = new[]
            {
                audience
            };
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            var client = new AuthenticationResult(new Client
            {
                ClientId = clientId
            }, null);
            var idtp = new JwsPayload
            {
                { JwtConstants.StandardResourceOwnerClaimNames.Subject, subject },
                { StandardClaimNames.Audiences, audiences }
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                IdTokenPayLoad = idtp,
                CreateDateTime = DateTime.UtcNow.AddDays(-2),
                ExpiresIn = 2
            };
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
                .Returns(() => Task.FromResult(client));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult(grantedToken));

                        var result = await _postIntrospectionAction.Execute(parameter, authenticationHeaderValue, null).ConfigureAwait(false);

                        Assert.NotNull(result);
            Assert.False(result.Active);
            Assert.True(result.Audience == audience);
            Assert.True(result.Subject == subject);
        }

        [Fact]
        public async Task When_Passing_Active_AccessToken_Then_Result_Should_Be_Returned()
        {            InitializeFakeObjects();
            const string clientId = "client_id";
            const string subject = "subject";
            const string audience = "audience";
            var authenticationHeaderValue = new AuthenticationHeaderValue("Basic", "ClientId:ClientSecret".Base64Encode());
            var audiences = new[]
            {
                audience
            };
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            var client = new AuthenticationResult(new Client
            {
                ClientId = clientId
            }, null);
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                IdTokenPayLoad = new JwsPayload
                {
                    {
                        JwtConstants.StandardResourceOwnerClaimNames.Subject,
                        subject
                    },
                    {
                        StandardClaimNames.Audiences,
                        audiences
                    }
                },
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 20000
            };
            _authenticateClientStub.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
                .Returns(Task.FromResult(client));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult(grantedToken));

                        var result = await _postIntrospectionAction.Execute(parameter, authenticationHeaderValue, null).ConfigureAwait(false);

                        Assert.NotNull(result);
            Assert.True(result.Active);
            Assert.True(result.Audience == audience);
            Assert.True(result.Subject == subject);
        }

        private void InitializeFakeObjects()
        {
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _authenticateClientStub = new Mock<IAuthenticateClient>();
            _introspectionParameterValidatorStub = new Mock<IIntrospectionParameterValidator>();
            _tokenStoreStub = new Mock<ITokenStore>();
            _postIntrospectionAction = new PostIntrospectionAction(
                _oauthEventSource.Object,
                _authenticateClientStub.Object,
                _introspectionParameterValidatorStub.Object,
                _tokenStoreStub.Object);
        }
    }
}
