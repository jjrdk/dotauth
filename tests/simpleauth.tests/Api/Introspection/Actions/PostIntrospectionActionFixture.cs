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

using SimpleAuth.Shared.Repositories;

namespace SimpleAuth.Tests.Api.Introspection.Actions
{
    using Errors;
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Introspection;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Xunit;
    using JwtConstants = Shared.JwtConstants;

    public class PostIntrospectionActionFixture
    {
        private Mock<IClientStore> _clientStore;
        private Mock<ITokenStore> _tokenStoreStub;
        private PostIntrospectionAction _postIntrospectionAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _postIntrospectionAction.Execute(null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new IntrospectionParameter
            {
                Token = "token"
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(new Client());
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //   .Returns(Task.FromResult(new AuthenticationResult(null, null)));

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _postIntrospectionAction.Execute(parameter, null, null)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
        }

        [Fact]
        public async Task When_AccessToken_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var parameter = new IntrospectionParameter
            {
                ClientId = "test",
                ClientSecret = "test",
                TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.AccessToken,
                Token = "token"
            };
            //var client = new AuthenticationResult(new Client(), null);
            _clientStore.Setup(x => x.GetById(It.IsAny<string>()))
                .ReturnsAsync(
                    new Client
                    {
                        ClientId = "test",
                        Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "test" } }
                    });

            var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", "test:test".Base64Encode());
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(client));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _postIntrospectionAction.Execute(parameter, authenticationHeaderValue, null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidToken, exception.Code);
            Assert.Equal(ErrorDescriptions.TheTokenIsNotValid, exception.Message);
        }

        [Fact]
        public async Task When_Passing_Expired_AccessToken_Then_Result_Should_Be_Returned()
        {
            InitializeFakeObjects();
            const string clientId = "client_id";
            const string clientSecret = "secret";
            const string subject = "subject";
            const string audience = "audience";
            var authenticationHeaderValue = new AuthenticationHeaderValue("Basic", $"{clientId}:{clientSecret}".Base64Encode());
            var audiences = new[]
            {
                audience
            };
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            var client = new Client
            {
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } }
            };
            var idtp = new JwtPayload
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
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(client));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult(grantedToken));

            var result = await _postIntrospectionAction.Execute(parameter, authenticationHeaderValue, null).ConfigureAwait(false);

            Assert.False(result.Active);
            Assert.Equal(audience, result.Audience);
            Assert.Equal(subject, result.Subject);
        }

        [Fact]
        public async Task When_Passing_Active_AccessToken_Then_Result_Should_Be_Returned()
        {
            InitializeFakeObjects();
            const string clientId = "client_id";
            const string clientSecret = "secret";
            const string subject = "subject";
            const string audience = "audience";
            var authenticationHeaderValue = new AuthenticationHeaderValue("Basic", $"{clientId}:{clientSecret}".Base64Encode());
            var audiences = new[]
            {
                audience
            };
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            var client = new Client
            {
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } }
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                IdTokenPayLoad = new JwtPayload
                {
                    {JwtConstants.StandardResourceOwnerClaimNames.Subject, subject},
                    {StandardClaimNames.Audiences, audiences}
                },
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 20000
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(client));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult(grantedToken));

            var result = await _postIntrospectionAction.Execute(parameter, authenticationHeaderValue, null).ConfigureAwait(false);

            Assert.True(result.Active);
            Assert.Equal(audience, result.Audience);
            Assert.Equal(subject, result.Subject);
        }

        private void InitializeFakeObjects()
        {
            _clientStore = new Mock<IClientStore>();
            _tokenStoreStub = new Mock<ITokenStore>();
            _postIntrospectionAction = new PostIntrospectionAction(
                _clientStore.Object,
                _tokenStoreStub.Object);
        }
    }
}
