namespace DotAuth.Tests;

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Tests.Helpers;
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

    [Theory]
    [MemberData(nameof(GetKeys))]
    public void SerializeJwk(JsonWebKey key)
    {
        using var stream = new System.IO.MemoryStream();
        var converter = new JwkConverter();
        using var utf8JsonWriter = new Utf8JsonWriter(stream);
        converter.Write(utf8JsonWriter, key, new JsonSerializerOptions());
        utf8JsonWriter.Flush();
        stream.Seek(0, System.IO.SeekOrigin.Begin);
        using var reader = new System.IO.StreamReader(stream);
        var json = reader.ReadToEnd();
        var utf8JsonReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        var jwk = converter.Read(ref utf8JsonReader, typeof(JsonWebKey), new JsonSerializerOptions());

        Assert.Equal(key.Kty, jwk.Kty);
        Assert.Equal(key.KeyOps, jwk.KeyOps);
    }

    public static IEnumerable<TheoryDataRow<JsonWebKey>> GetKeys()
    {
        yield return new TheoryDataRow<JsonWebKey>(TestKeys.SecretKey.CreateJwk(JsonWebKeyUseNames.Sig,
            KeyOperations.Sign, KeyOperations.Verify));
        using var rsa = RSA.Create();
        yield return new TheoryDataRow<JsonWebKey>(rsa.CreateJwk("1", JsonWebKeyUseNames.Sig, true, KeyOperations.Sign,
            KeyOperations.Verify));
        using var ecdsa = ECDsa.Create();
        yield return new TheoryDataRow<JsonWebKey>(ecdsa.CreateJwk("1", JsonWebKeyUseNames.Sig, true,
            KeyOperations.Sign, KeyOperations.Verify));
    }
}
