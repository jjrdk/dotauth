using Microsoft.Extensions.Caching.Redis;
using SimpleIdentityServer.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static SimpleIdentityServer.AccessToken.Store.Redis.RedisTokenStore;

namespace SimpleIdentityServer.AccessToken.Store.Redis.Integration.Tests
{
    public class AccessTokenFixture
    {
        private RedisStorage _redisStorage;
        private IAccessTokenStore _accessTokenStore;

        [Fact]
        public async Task When_Get_Two_Access_Tokens_Then_Second_Is_Coming_From_Redis_Cache()
        {
            // ARRANGE
            const string url = "http://localhost:60000/.well-known/openid-configuration";
            InitializeFakeObjects();
            var val = await _redisStorage.GetValue(url).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(val))
            {
                await _redisStorage.RemoveAsync(url).ConfigureAwait(false);
            }

            // ACT
            var grantedToken = await _accessTokenStore.GetToken(url, "MedicalWebsite", "MedicalWebsite", new[]
            {
                "uma_protection"
            }).ConfigureAwait(false);
            var firstTokens = await _redisStorage.TryGetValueAsync<List<StoredToken>>(url).ConfigureAwait(false);
            var secondGrantedToken = await _accessTokenStore.GetToken(url, "MedicalWebsite", "MedicalWebsite", new[]
            {
                "uma_protection"
            }).ConfigureAwait(false);
            var secondTokens = await _redisStorage.TryGetValueAsync<List<StoredToken>>(url).ConfigureAwait(false);
            var thirdGrantedToken = await _accessTokenStore.GetToken(url, "MedicalWebsite", "MedicalWebsite", new[]
            {
                "uma_protection",
                "uma_authorization"
            }).ConfigureAwait(false);
            var thirdTokens = await _redisStorage.TryGetValueAsync<List<StoredToken>>(url).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(firstTokens.Count(), 1);
            Assert.Equal(secondTokens.Count(), 1);
            Assert.Equal(thirdTokens.Count(), 2);
        }

        private void InitializeFakeObjects()
        {
            _redisStorage = new RedisStorage(new RedisCacheOptions
            {
                Configuration = "127.0.0.1",
                InstanceName = "SID"
            }, 6379);
            _accessTokenStore = new RedisTokenStore(_redisStorage, new IdentityServerClientFactory());
        }
    }
}
