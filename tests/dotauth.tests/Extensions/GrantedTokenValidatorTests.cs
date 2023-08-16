namespace DotAuth.Tests.Extensions;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

public sealed class GrantedTokenValidatorTests
{
    static GrantedTokenValidatorTests()
    {
        IdentityModelEventSource.ShowPII = true;
    }

    [Fact]
    public async Task WhenTokenIsSignedByKeyIsJwksStoreThenTokenIsValid()
    {
        var handler = new JwtSecurityTokenHandler();
        using var rsa = new RSACryptoServiceProvider(2048);
        var jwk = rsa.CreateSignatureJwk("1", true);
        var keyset = new JsonWebKeySet().AddKey(rsa.CreateSignatureJwk("1", false));
        var jwksStoreMock = Substitute.For<IJwksStore>();
        jwksStoreMock.GetSigningKey(jwk.Alg, Arg.Any<CancellationToken>())
            .Returns(new SigningCredentials(jwk, jwk.Alg));
        jwksStoreMock.GetPublicKeys(Arg.Any<CancellationToken>()).Returns(keyset);
        var token = handler.CreateEncodedJwt(
            "http://localhost",
            "test",
            new ClaimsIdentity(new[] { new Claim("sub", "tester"), }),
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            DateTime.UtcNow,
            new SigningCredentials(jwk, jwk.Alg));
        var grantedToken = new GrantedToken
        {
            ClientId = "test",
            AccessToken = token,
            ExpiresIn = 10000,
            CreateDateTime = DateTimeOffset.UtcNow
        };
        var result = await grantedToken.CheckGrantedToken(jwksStoreMock).ConfigureAwait(false);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task WhenTokenIsForDifferentAudienceThenTokenIsNotValid()
    {
        var handler = new JwtSecurityTokenHandler();
        using var rsa = new RSACryptoServiceProvider(2048);
        var jwk = rsa.CreateSignatureJwk("1", true);
        var keyset = new JsonWebKeySet().AddKey(rsa.CreateSignatureJwk("1", false));
        var jwksStoreMock = Substitute.For<IJwksStore>();
        jwksStoreMock.GetSigningKey(jwk.Alg, Arg.Any<CancellationToken>())
            .Returns(new SigningCredentials(jwk, jwk.Alg));
        jwksStoreMock.GetPublicKeys(Arg.Any<CancellationToken>()).Returns(keyset);
        var token = handler.CreateEncodedJwt(
            "http://localhost",
            "test",
            new ClaimsIdentity(new[] { new Claim("sub", "tester"), }),
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            DateTime.UtcNow,
            new SigningCredentials(jwk, jwk.Alg));
        var grantedToken = new GrantedToken
        {
            ClientId = "fake",
            AccessToken = token,
            ExpiresIn = 10000,
            CreateDateTime = DateTimeOffset.UtcNow
        };
        var result = await grantedToken.CheckGrantedToken(jwksStoreMock).ConfigureAwait(false);

        Assert.False(result.IsValid);
    }
}
