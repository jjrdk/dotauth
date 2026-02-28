namespace DotAuth.Tests;

using System.Threading.Tasks;
using DotAuth.Repositories;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public sealed class InMemoryJwksRepositoryFixture
{
    private readonly InMemoryJwksRepository _repository = new();

    [Fact]
    public async Task WhenGettingPublicKeysThenHasTwoKeys()
    {
        var publicKeys = await _repository.GetPublicKeys(TestContext.Current.CancellationToken);

        Assert.Equal(2, publicKeys!.Keys.Count);
    }

    [Fact]
    public async Task WhenGettingPublicKeysThenThereAreNoPrivateKeys()
    {
        var publicKeys = await _repository.GetPublicKeys(TestContext.Current.CancellationToken);

        Assert.All(publicKeys!.Keys, jwk => Assert.False(jwk.HasPrivateKey));
    }

    [Fact]
    public async Task WhenGettingSigningKeyThenReturnsKey()
    {
        var signingKey =
            await _repository.GetSigningKey(SecurityAlgorithms.RsaSha256, TestContext.Current.CancellationToken);

        Assert.NotNull(signingKey);
    }
}
