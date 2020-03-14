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
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Introspection;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Errors;
    using Xunit;

    public class PostIntrospectionActionFixture
    {
        private readonly Mock<IClientStore> _clientStore;
        private readonly Mock<ITokenStore> _tokenStoreStub;
        private readonly PostIntrospectionAction _postIntrospectionAction;

        public PostIntrospectionActionFixture()
        {
            _clientStore = new Mock<IClientStore>();
            _tokenStoreStub = new Mock<ITokenStore>();
            _postIntrospectionAction = new PostIntrospectionAction(_clientStore.Object, _tokenStoreStub.Object, new InMemoryJwksRepository());
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<NullReferenceException>(
                    () => _postIntrospectionAction.Execute(null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            var parameter = new IntrospectionParameter { Token = "token" };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Client());

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _postIntrospectionAction.Execute(parameter, null, null, CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
        }

        [Fact]
        public async Task When_AccessToken_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {
            var parameter = new IntrospectionParameter
            {
                ClientId = "test",
                ClientSecret = "test",
                TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.AccessToken,
                Token = "token"
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new Client
                    {
                        ClientId = "test",
                        Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "test" } }
                    });

            var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", "test:test".Base64Encode());

            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _postIntrospectionAction.Execute(
                        parameter,
                        authenticationHeaderValue,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidToken, exception.Code);
            Assert.Equal(ErrorDescriptions.TheTokenIsNotValid, exception.Message);
        }

        [Fact]
        public async Task When_Passing_Expired_AccessToken_Then_Result_Should_Be_Returned()
        {
            const string clientId = "client_id";
            const string clientSecret = "secret";
            const string subject = "subject";
            const string audience = "audience";
            var authenticationHeaderValue = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var audiences = new[] { audience };
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            var client = new Client
            {
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } }
            };
            var idtp = new JwtPayload
            {
                {OpenIdClaimTypes.Subject, subject},
                {StandardClaimNames.Audiences, audiences}
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                IdTokenPayLoad = idtp,
                CreateDateTime = DateTimeOffset.UtcNow.AddDays(-2),
                ExpiresIn = 2
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(grantedToken));

            var result = await _postIntrospectionAction
                .Execute(parameter, authenticationHeaderValue, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result.Active);
            Assert.Equal(audience, result.Audience);
            Assert.Equal(subject, result.Subject);
        }

        [Fact]
        public async Task When_Passing_Active_AccessToken_Then_Result_Should_Be_Returned()
        {
            const string clientId = "client_id";
            const string clientSecret = "secret";
            const string subject = "subject";
            const string audience = "audience";
            var authenticationHeaderValue = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var audiences = new[] { audience };
            var parameter = new IntrospectionParameter
            {
                TokenTypeHint = CoreConstants.StandardTokenTypeHintNames.RefreshToken,
                Token = "token"
            };
            var client = new Client
            {
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } }
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                IdTokenPayLoad = new JwtPayload
                {
                    {OpenIdClaimTypes.Subject, subject},
                    {StandardClaimNames.Audiences, audiences}
                },
                CreateDateTime = DateTimeOffset.UtcNow,
                ExpiresIn = 20000
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _tokenStoreStub.Setup(a => a.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _tokenStoreStub.Setup(a => a.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(grantedToken));

            var result = await _postIntrospectionAction
                .Execute(parameter, authenticationHeaderValue, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.True(result.Active);
            Assert.Equal(audience, result.Audience);
            Assert.Equal(subject, result.Subject);
        }
    }
}
