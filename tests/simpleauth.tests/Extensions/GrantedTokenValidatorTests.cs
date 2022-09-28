namespace SimpleAuth.Tests.Extensions;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SimpleAuth.Extensions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Repositories;
using Xunit;

public sealed class GrantedTokenValidatorTests
{
    static GrantedTokenValidatorTests()
    {
        IdentityModelEventSource.ShowPII = true;
    }

    [Fact]
    public async Task WhenCheckingNullTokenThenTokenIsNotValid()
    {
        var jwksStoreMock = new Mock<IJwksStore>();
        GrantedToken token = null;
        var result = await token.CheckGrantedToken(jwksStoreMock.Object).ConfigureAwait(false);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task WhenTokenIsSignedByKeyIsJwksStoreThenTokenIsValid()
    {
        var handler = new JwtSecurityTokenHandler();
        using var rsa = new RSACryptoServiceProvider(2048);
        var jwk = rsa.CreateSignatureJwk("1", true);
        var keyset = new JsonWebKeySet().AddKey(rsa.CreateSignatureJwk("1", false));
        var jwksStoreMock = new Mock<IJwksStore>();
        jwksStoreMock.Setup(x => x.GetSigningKey(jwk.Alg, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SigningCredentials(jwk, jwk.Alg));
        jwksStoreMock.Setup(x => x.GetPublicKeys(It.IsAny<CancellationToken>())).ReturnsAsync(keyset);
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
        var result = await grantedToken.CheckGrantedToken(jwksStoreMock.Object).ConfigureAwait(false);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task WhenTokenIsForDifferentAudienceThenTokenIsNotValid()
    {
        var handler = new JwtSecurityTokenHandler();
        using var rsa = new RSACryptoServiceProvider(2048);
        var jwk = rsa.CreateSignatureJwk("1", true);
        var keyset = new JsonWebKeySet().AddKey(rsa.CreateSignatureJwk("1", false));
        var jwksStoreMock = new Mock<IJwksStore>();
        jwksStoreMock.Setup(x => x.GetSigningKey(jwk.Alg, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SigningCredentials(jwk, jwk.Alg));
        jwksStoreMock.Setup(x => x.GetPublicKeys(It.IsAny<CancellationToken>())).ReturnsAsync(keyset);
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
        var result = await grantedToken.CheckGrantedToken(jwksStoreMock.Object).ConfigureAwait(false);

        Assert.False(result.IsValid);
    }
}