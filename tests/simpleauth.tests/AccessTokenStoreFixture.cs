//using Moq;
//using SimpleIdentityServer.AccessToken.Store;
//using SimpleIdentityServer.Client;
//using SimpleIdentityServer.Client.Results;
//using SimpleIdentityServer.Client.Selectors;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;

//namespace SimpleIdentityServer.AccessToken.Store.Tests
//{
//    public class AccessTokenStoreFixture
//    {
//        private Mock<IIdentityServerClientFactory> _identityServerClientFactoryStub;
//        private InMemoryTokenStore _accessTokenStore;

//        [Fact]
//        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
//        {
////            InitializeFakeObjects();

//            //            await Assert.ThrowsAsync<ArgumentNullException>(() => _accessTokenStore.GetToken(null, null, null, null));
//            await Assert.ThrowsAsync<ArgumentNullException>(() => _accessTokenStore.GetToken("url", null, null, null));
//            await Assert.ThrowsAsync<ArgumentNullException>(() => _accessTokenStore.GetToken("url", "clientid", null, null));
//            await Assert.ThrowsAsync<ArgumentNullException>(() => _accessTokenStore.GetToken("url", "clientid", "clientsecret", null));
//        }

//        [Fact]
//        public async Task When_Get_AccessToken_Then_NewOne_Is_Inserted_In_The_Cache()
//        {
////            InitializeFakeObjects();
//            var tokenClient = new Mock<ITokenClient>();
//            tokenClient.Setup(t => t.ResolveAsync(It.IsAny<string>()))
//                .ReturnsAsync(new GetTokenResult
//                {
//                    Content = new Core.Common.DTOs.Responses.GrantedTokenResponse
//                    {
//                        ExpiresIn = 3600
//                    }
//                }));
//            var tokenGrantTypeSelector = new Mock<ITokenGrantTypeSelector>();
//            tokenGrantTypeSelector.Setup(c => c.UseClientCredentials(It.IsAny<string[]>()))
//                .Returns(tokenClient.Object);
//            var clientAuthSelector = new Mock<IClientAuthSelector>();
//            clientAuthSelector.Setup(c => c.UseClientSecretPostAuth(It.IsAny<string>(), It.IsAny<string>()))
//                .Returns(tokenGrantTypeSelector.Object);
//            _identityServerClientFactoryStub.Setup(i => i.CreateAuthSelector()).Returns(clientAuthSelector.Object);

//            //            var result = await _accessTokenStore.GetToken("url", "clientid", "clientsecret", new[] { "scope" }).ConfigureAwait(false);
//            var cachedTokens = _accessTokenStore.Tokens.Count();
//            var secondResult = await _accessTokenStore.GetToken("url", "clientid", "clientsecret", new[] { "scope" }).ConfigureAwait(false);
//            var secondCachedTokens = _accessTokenStore.Tokens.Count();
//            var thirdResult = await _accessTokenStore.GetToken("url", "clientid", "clientsecret", new[] { "scope", "scope2" }).ConfigureAwait(false);
//            var thirdCachedToken = _accessTokenStore.Tokens.Count();

//            //            Assert.Equal(cachedTokens, 1);
//            Assert.Equal(secondCachedTokens, 1);
//            Assert.Equal(thirdCachedToken, 2);
//        }

//        private void InitializeFakeObjects()
//        {
//            _identityServerClientFactoryStub = new Mock<IIdentityServerClientFactory>();
//            _accessTokenStore = new InMemoryTokenStore(_identityServerClientFactoryStub.Object);
//        }
//    }
//}
