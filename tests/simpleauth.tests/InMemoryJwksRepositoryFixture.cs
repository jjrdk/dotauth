namespace SimpleAuth.Tests
{
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using Xunit;

    public class InMemoryJwksRepositoryFixture
    {
        private readonly InMemoryJwksRepository _repository;

        public InMemoryJwksRepositoryFixture()
        {
            _repository = new InMemoryJwksRepository();
        }

        [Fact]
        public async Task WhenGettingPublicKeysThenHasTwoKeys()
        {
            var publicKeys = await _repository.GetPublicKeys().ConfigureAwait(false);

            Assert.Equal(2, publicKeys.Keys.Count);
        }

        [Fact]
        public async Task WhenGettingPublicKeysThenThereAreNoPrivateKeys()
        {
            var publicKeys = await _repository.GetPublicKeys().ConfigureAwait(false);

            Assert.All(publicKeys.Keys, jwk => Assert.False(jwk.HasPrivateKey));
        }

        [Fact]
        public async Task WhenGettingSigningKeyThenReturnsKey()
        {
            var signingKey = await _repository.GetSigningKey(SecurityAlgorithms.RsaSha256).ConfigureAwait(false);

            Assert.NotNull(signingKey);
        }
    }
}
