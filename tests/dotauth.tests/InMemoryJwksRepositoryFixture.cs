namespace DotAuth.Tests;

using System.Threading.Tasks;
using DotAuth.Repositories;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public sealed class InMemoryJwksRepositoryFixture
{
    private readonly InMemoryJwksRepository _repository;

    public InMemoryJwksRepositoryFixture()
    {
        _repository = new InMemoryJwksRepository();
    }

    [Fact]
    public async Task WhenGettingPublicKeysThenHasTwoKeys()
    {
        var publicKeys = await _repository.GetPublicKeys();

        Assert.Equal(2, publicKeys.Keys.Count);
    }

    [Fact]
    public async Task WhenGettingPublicKeysThenThereAreNoPrivateKeys()
    {
        var publicKeys = await _repository.GetPublicKeys();

        Assert.All(publicKeys.Keys, jwk => Assert.False(jwk.HasPrivateKey));
    }

    [Fact]
    public async Task WhenGettingSigningKeyThenReturnsKey()
    {
        var signingKey = await _repository.GetSigningKey(SecurityAlgorithms.RsaSha256);

        Assert.NotNull(signingKey);
    }
}
