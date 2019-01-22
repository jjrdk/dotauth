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

using Microsoft.IdentityModel.Tokens;
using SimpleAuth.Shared.Repositories;

namespace SimpleAuth.Tests.Common
{
    using Logging;
    using Moq;
    using Parameters;
    using Results;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Common;
    using SimpleAuth.JwtToken;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class GenerateAuthorizationResponseFixture
    {
        private Mock<IAuthorizationCodeStore> _authorizationCodeRepositoryFake;
        private Mock<IJwtGenerator> _jwtGeneratorFake;
        private Mock<ITokenStore> _tokenStore;
        private Mock<IEventPublisher> _eventPublisher;
        private IGenerateAuthorizationResponse _generateAuthorizationResponse;
        private Mock<IClientStore> _clientStore;
        private Mock<IConsentRepository> _consentRepository;

        public GenerateAuthorizationResponseFixture()
        {
            InitializeFakeObjects();
        }

        public static string ToHexString(IEnumerable<byte> arr)
        {
            if (arr == null)
            {
                throw new ArgumentNullException(nameof(arr));
            }

            var sb = new StringBuilder();
            foreach (var s in arr)
            {
                sb.Append(s.ToString("x2"));
            }

            return sb.ToString();
        }

        [Fact]
        public void WhenGenerateSessionState()
        {
            // 0cb582e736597d717f0f6b34b987ea5ad0a6a82c1294fa37e2d75a444da782aa
            // 0cb582e736597d717f0f6b34b987ea5ad0a6a82c1294fa37e2d75a444da782aa
            const string clientId = "ResourceManagerClientId";
            const string originUrl = "http://localhost:64950";
            const string sessionId = "d95d6ea3-36f5-4ccd-886a-d469210f8e33";
            const string salt = "a781f21b-a9e0-4b84-90ed-2dc4535ac927";
            var bytes = Encoding.UTF8.GetBytes(clientId + originUrl + sessionId + salt);
            byte[] hash;
            using (var sha = SHA256.Create())
            {
                hash = sha.ComputeHash(bytes);
            }

            var hashed = ToHexString(hash);
            var b = hashed.Base64Encode();
            //string s = "";
        }

        [Fact]
        public async Task When_Passing_No_Action_Result_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _generateAuthorizationResponse.Generate(null, null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Authorization_Request_Then_Exception_Is_Thrown()
        {
            var redirectInstruction = new EndpointResult { RedirectInstruction = new RedirectInstruction() };

            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _generateAuthorizationResponse.Generate(redirectInstruction, null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_There_Is_No_Logged_User_Then_Exception_Is_Throw()
        {
            var redirectInstruction = new EndpointResult { RedirectInstruction = new RedirectInstruction() };

            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _generateAuthorizationResponse.Generate(
                        redirectInstruction,
                        new AuthorizationParameter(),
                        null,
                        null,
                        null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_No_Client_Is_Passed_Then_Exception_Is_Thrown()
        {
            var redirectInstruction = new EndpointResult { RedirectInstruction = new RedirectInstruction() };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));

            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _generateAuthorizationResponse.Generate(
                        redirectInstruction,
                        new AuthorizationParameter(),
                        claimsPrincipal,
                        null,
                        null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Generating_AuthorizationResponse_With_IdToken_Then_IdToken_Is_Added_To_The_Parameters()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            // const string idToken = "idToken";
            var clientId = "client";
            var authorizationParameter =
                new AuthorizationParameter { ResponseType = ResponseTypeNames.IdToken, ClientId = clientId };
            var actionResult = new EndpointResult { RedirectInstruction = new RedirectInstruction() };
            var jwsPayload = new JwtPayload();

            _jwtGeneratorFake.Setup(
                    j => j.GenerateIdTokenPayloadForScopesAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>(),
                        null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                    j => j.GenerateUserInfoPayloadForScopeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(jwsPayload));
            //_jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<JwtPayload>(), It.IsAny<string>(), It.IsAny<string>()))
            //    .Returns(Task.FromResult(idToken));
            //_clientHelperFake.Setup(c => c.GenerateIdTokenAsync(It.IsAny<string>(), It.IsAny<JwtPayload>()))
            //    .Returns(Task.FromResult(idToken));

            var client = new Client
            {
                ClientId = clientId,
                JsonWebKeys =
                    "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                        .ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    client,
                    null)
                .ConfigureAwait(false);

            Assert.Contains(
                actionResult.RedirectInstruction.Parameters,
                p => p.Name == CoreConstants.StandardAuthorizationResponseNames.IdTokenName);
        }

        [Fact]
        public async Task
            When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_No_Granted_Token_Then_Token_Is_Generated_And_Added_To_The_Parameters()
        {
            //const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Token,
                ClientId = clientId,
                Scope = scope
            };

            var client = new Client
            {
                ClientId = clientId,
                JsonWebKeys = "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify).ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256
            };
            //var grantedToken = new GrantedToken { AccessToken = Id.Create() };
            var actionResult = new EndpointResult { RedirectInstruction = new RedirectInstruction() };
            var jwsPayload = new JwtPayload();
            //_parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
            //    .Returns(new[] { ResponseTypeNames.Token });
            _jwtGeneratorFake.Setup(
                    j => j.GenerateIdTokenPayloadForScopesAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>(),
                        null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                    j => j.GenerateUserInfoPayloadForScopeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(jwsPayload));

            //_jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<JwtPayload>(), It.IsAny<string>(), It.IsAny<string>()))
            //    .Returns(Task.FromResult(idToken));
            //_parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
            //    .Returns(() => new List<string> { scope });
            //_grantedTokenHelperStub
            //    .Setup(
            //        r => r.GetValidGrantedTokenAsync(
            //            It.IsAny<string>(),
            //            It.IsAny<string>(),
            //            It.IsAny<JwtPayload>(),
            //            It.IsAny<JwtPayload>()))
            //    .Returns(Task.FromResult((GrantedToken)null));

            //_grantedTokenGeneratorHelperFake
            //    .Setup(
            //        r => r.GenerateToken(
            //            It.IsAny<Client>(),
            //            It.IsAny<string>(),
            //            It.IsAny<string>(),
            //            It.IsAny<IDictionary<string, object>>(),
            //            It.IsAny<JwtPayload>(),
            //            It.IsAny<JwtPayload>()))
            //    .Returns(Task.FromResult(grantedToken));

            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    client,
                    null)
                .ConfigureAwait(false);

            Assert.Contains(
                actionResult.RedirectInstruction.Parameters,
                p => p.Name == CoreConstants.StandardAuthorizationResponseNames.AccessTokenName);
            //Assert.Contains(actionResult.RedirectInstruction.Parameters, p => p.Value == grantedToken.AccessToken);
            _tokenStore.Verify(g => g.AddToken(It.IsAny<GrantedToken>()));
            _eventPublisher.Verify(e => e.Publish(It.IsAny<AccessToClientGranted>()));
        }

        [Fact]
        public async Task
            When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_A_GrantedToken_Then_Token_Is_Added_To_The_Parameters()
        {
            //const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Token,
                ClientId = clientId,
                Scope = scope
            };
            var grantedToken = new GrantedToken { AccessToken = Id.Create(), CreateDateTime = DateTime.UtcNow, ExpiresIn = 10000 };
            var actionResult = new EndpointResult { RedirectInstruction = new RedirectInstruction() };
            var jwsPayload = new JwtPayload();
            //_parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
            //    .Returns(new[] { ResponseTypeNames.Token });
            _jwtGeneratorFake.Setup(
                    j => j.GenerateIdTokenPayloadForScopesAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>(),
                        null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                    j => j.GenerateUserInfoPayloadForScopeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(jwsPayload));
            //_jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<JwtPayload>(), It.IsAny<string>(), It.IsAny<string>()))
            //    .Returns(Task.FromResult(idToken));
            //_parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
            //    .Returns(() => new List<string> { scope });
            //_grantedTokenHelperStub
            //    .Setup(
            //        r => r.GetValidGrantedTokenAsync(
            //            It.IsAny<string>(),
            //            It.IsAny<string>(),
            //            It.IsAny<JwtPayload>(),
            //            It.IsAny<JwtPayload>()))
            //    .Returns(() => Task.FromResult(grantedToken));
            _tokenStore.Setup(
                x => x.GetToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<JwtPayload>(),
                    It.IsAny<JwtPayload>()))
                .ReturnsAsync(grantedToken);
            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    new Client { ClientId = "client" },
                    null)
                .ConfigureAwait(false);

            Assert.Contains(
                actionResult.RedirectInstruction.Parameters,
                p => p.Name == CoreConstants.StandardAuthorizationResponseNames.AccessTokenName);
            Assert.Contains(actionResult.RedirectInstruction.Parameters, p => p.Value == grantedToken.AccessToken);
        }

        [Fact]
        public async Task
            When_Generating_AuthorizationResponse_With_AuthorizationCode_Then_Code_Is_Added_To_The_Parameters()
        {
            //const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "test"), }, "fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Code,
                ClientId = clientId,
                Scope = scope
            };

            var consent = new Consent
            {
                GrantedScopes = new List<Scope> { new Scope { Name = scope } },
                Client = new Client
                {
                    ClientId = clientId,
                    AllowedScopes = new List<Scope> { new Scope { Name = scope } }
                }
            };
            var actionResult = new EndpointResult { RedirectInstruction = new RedirectInstruction() };
            var jwsPayload = new JwtPayload();
            //_parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
            //    .Returns(new[] { ResponseTypeNames.Code });
            _jwtGeneratorFake.Setup(
                    j => j.GenerateIdTokenPayloadForScopesAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>(),
                        null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                    j => j.GenerateUserInfoPayloadForScopeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(jwsPayload));
            _consentRepository.Setup(x => x.GetConsentsForGivenUser(It.IsAny<string>())).ReturnsAsync(new[] { consent });
            //_jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<JwtPayload>(), It.IsAny<string>(), It.IsAny<string>()))
            //    .Returns(Task.FromResult(idToken));
            //_consentHelperFake
            //    .Setup(c => c.GetConfirmedConsents(It.IsAny<string>(), It.IsAny<AuthorizationParameter>()))
            //    .Returns(Task.FromResult(consent));

            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    new Client(),
                    null)
                .ConfigureAwait(false);

            Assert.Contains(
                actionResult.RedirectInstruction.Parameters,
                p => p.Name == CoreConstants.StandardAuthorizationResponseNames.AuthorizationCodeName);
            _authorizationCodeRepositoryFake.Verify(a => a.AddAuthorizationCode(It.IsAny<AuthorizationCode>()));
            _eventPublisher.Verify(s => s.Publish(It.IsAny<AuthorizationCodeGranted>()));
        }

        [Fact]
        public async Task
            When_Redirecting_To_Callback_And_There_Is_No_Response_Mode_Specified_Then_The_Response_Mode_Is_Set()
        {
            //const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "scope";
            const string responseType = "id_token";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope,
                ResponseType = responseType,
                ResponseMode = ResponseMode.None
            };
            //var client = new Client
            //{
            //    IdTokenEncryptedResponseAlg = SecurityAlgorithms.RsaPKCS1,
            //    IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
            //    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256
            //};
            var actionResult = new EndpointResult
            {
                RedirectInstruction = new RedirectInstruction(),
                Type = TypeActionResult.RedirectToCallBackUrl
            };
            var jwsPayload = new JwtPayload();
            //_parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
            //    .Returns(new[] { ResponseTypeNames.IdToken });
            _jwtGeneratorFake.Setup(
                    j => j.GenerateIdTokenPayloadForScopesAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>(),
                        null))
                .Returns(Task.FromResult(jwsPayload));
            _jwtGeneratorFake.Setup(
                    j => j.GenerateUserInfoPayloadForScopeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(jwsPayload));
            //_jwtGeneratorFake.Setup(j => j.EncryptAsync(It.IsAny<JwtPayload>(), It.IsAny<string>(), It.IsAny<string>()))
            //    .Returns(Task.FromResult(idToken));
            //_authorizationFlowHelperFake.Setup(
            //        a => a.GetAuthorizationFlow(It.IsAny<ICollection<string>>(), It.IsAny<string>()))
            //    .Returns(AuthorizationFlow.ImplicitFlow);

            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    new Client(),
                    null)
                .ConfigureAwait(false);

            Assert.Equal(ResponseMode.fragment, actionResult.RedirectInstruction.ResponseMode);
        }

        private void InitializeFakeObjects()
        {
            _authorizationCodeRepositoryFake = new Mock<IAuthorizationCodeStore>();
            _jwtGeneratorFake = new Mock<IJwtGenerator>();
            _tokenStore = new Mock<ITokenStore>();
            _eventPublisher = new Mock<IEventPublisher>();
            _eventPublisher.Setup(x => x.Publish(It.IsAny<AuthorizationCodeGranted>())).Returns(Task.CompletedTask);
            _eventPublisher.Setup(x => x.Publish(It.IsAny<AccessToClientGranted>())).Returns(Task.CompletedTask);
            _clientStore = new Mock<IClientStore>();
            _consentRepository = new Mock<IConsentRepository>();
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                _authorizationCodeRepositoryFake.Object,
                _tokenStore.Object,
                _jwtGeneratorFake.Object,
                _eventPublisher.Object,
                _clientStore.Object,
                _consentRepository.Object);
        }
    }
}
