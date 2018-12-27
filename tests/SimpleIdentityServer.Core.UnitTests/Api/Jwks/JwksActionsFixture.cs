namespace SimpleIdentityServer.Core.UnitTests.Api.Jwks
{
    using Moq;
    using SimpleAuth;
    using SimpleAuth.Api.Jwks;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class JwksActionsFixture
    {
        private IJwksActions _jwksActions;

        [Fact]
        public async Task When_There_Is_No_JsonWebKeys_To_Rotate_Then_False_Is_Returned()
        {
            InitializeFakeObjects(null);
            //_jsonWebKeyRepositoryStub.Setup(j => j.GetAllAsync())
            //    .Returns(() => Task.FromResult((ICollection<JsonWebKey>)null));

            var result = await _jwksActions.RotateJwks().ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task When_Rotating_Two_JsonWebKeys_Then_SerializedKeyProperty_Has_Changed()
        {
            const string firstJsonWebKeySerializedKey = "firstJsonWebKeySerializedKey";
            const string secondJsonWebKeySerializedKey = "secondJsonWebKeySerializedKey";
            var jsonWebKeys = new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Kid = "1",
                    SerializedKey = firstJsonWebKeySerializedKey
                },
                new JsonWebKey
                {
                    Kid = "2",
                    SerializedKey = secondJsonWebKeySerializedKey
                }
            };

            InitializeFakeObjects(jsonWebKeys);
            //_jsonWebKeyRepositoryStub.Setup(j => j.GetAllAsync())
            //    .Returns(() => Task.FromResult(jsonWebKeys));

            var result = await _jwksActions.RotateJwks().ConfigureAwait(false);

            //_jsonWebKeyRepositoryStub.Verify(j => j.UpdateAsync(It.IsAny<JsonWebKey>()));
            Assert.True(result);
        }

        [Fact]
        public async Task When_Retrieving_Jwks_Then_Set_Of_Private_And_Public_Keys_Are_Returned()
        {
            InitializeFakeObjects();

            var result = await _jwksActions.GetJwks().ConfigureAwait(false);

            Assert.NotNull(result);
        }

        private void InitializeFakeObjects(IReadOnlyCollection<JsonWebKey> jsonWebKeys = null)
        {
            _jwksActions = new JwksActions(
                new DefaultJsonWebKeyRepository(jsonWebKeys ?? new JsonWebKey[0]),
                new InMemoryTokenStore());
        }
    }
}
