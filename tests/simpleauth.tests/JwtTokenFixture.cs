namespace SimpleAuth.Tests;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public static class JwtTokenFixture
{
    public sealed class GivenAJwtTokenHandler
    {
        private readonly JwtSecurityTokenHandler _handler;

        public GivenAJwtTokenHandler()
        {
            _handler = new JwtSecurityTokenHandler();
        }

        [Fact]
        public void CanCreateValidToken()
        {
            var token = _handler.CreateEncodedJwt(
                "issuer",
                "audience",
                null,
                null,
                DateTime.UtcNow.AddSeconds(3600),
                DateTime.UtcNow,
                new SigningCredentials(new RsaSecurityKey(RSA.Create()), SecurityAlgorithms.RsaSha256));

            Assert.Equal(3, token.Split('.').Length);
        }
    }
}