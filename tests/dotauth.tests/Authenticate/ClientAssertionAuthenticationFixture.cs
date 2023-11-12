namespace DotAuth.Tests.Authenticate;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Authenticate;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Tests.Helpers;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

public sealed class ClientAssertionAuthenticationFixture
{
    private readonly JwtSecurityTokenHandler _handler = new();
    private readonly IClientStore _clientRepositoryStub;
    private readonly ClientAssertionAuthentication _clientAssertionAuthentication;

    public static IEnumerable<object[]> InvalidPayloads()
    {
        return new[]
        {
            new[]
            {
                new JwtPayload
                {
                    {StandardClaimNames.Issuer, "issuer"},
                    {StandardClaimNames.Subject, "issuer"},
                    {StandardClaimNames.Audiences, "audience"}
                }
            },
            new[]
            {
                new JwtPayload
                {
                    {StandardClaimNames.Issuer, "issuer"},
                    {StandardClaimNames.Subject, "issuer"},
                    {StandardClaimNames.Audiences, "audience"}
                }
            },
            new[]
            {
                new JwtPayload
                {
                    {StandardClaimNames.Issuer, "issuer"},
                    {StandardClaimNames.Subject, "issuer"},
                    {StandardClaimNames.Audiences, "audience"},
                    {
                        StandardClaimNames.ExpirationTime,
                        DateTime.Now.AddDays(-2).ConvertToUnixTimestamp()
                    }
                }
            }
        };
    }

    public ClientAssertionAuthenticationFixture()
    {
        _clientRepositoryStub = Substitute.For<IClientStore>();
        _clientAssertionAuthentication = new ClientAssertionAuthentication(
            _clientRepositoryStub,
            new InMemoryJwksRepository());
    }

    [Fact]
    public async Task When_A_Not_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
    {
        var instruction = new AuthenticateInstruction {ClientAssertion = "invalid_header.invalid_payload"};
        var result = await _clientAssertionAuthentication
            .AuthenticateClientWithPrivateKeyJwt(instruction, "", CancellationToken.None);

        Assert.Null(result.Client);
        Assert.Equal(Strings.TheClientAssertionIsNotAJwsToken, result.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidPayloads))]
    public async Task WhenInvalidJwtIsPassedThenReturnsNullClient(JwtPayload jwsPayload)
    {
        var jwks = CreateJwt(jwsPayload, out var jwt);
        var instruction = new AuthenticateInstruction
        {
            ClientAssertion = jwt // "invalid_header.invalid_payload"
        };
        var client = new Client {JsonWebKeys = jwks};

        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);

        var result = await _clientAssertionAuthentication
            .AuthenticateClientWithPrivateKeyJwt(instruction, "invalid_issuer", CancellationToken.None)
            ;

        Assert.Null(result.Client);
        Assert.NotNull(result.ErrorMessage);
    }

    private JsonWebKeySet CreateJwt(JwtPayload jwsPayload, out string jwt)
    {
        var jwks = TestKeys.SecretKey.CreateSignatureJwk().ToSet();

        var token = new JwtSecurityToken(
            new JwtHeader(new SigningCredentials(jwks.Keys[0], SecurityAlgorithms.HmacSha256)),
            jwsPayload);
        jwt = _handler.WriteToken(token);
        return jwks;
    }

    [Fact]
    public async Task
        When_A_Valid_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Client_Is_Returned()
    {
        var jwsPayload = new JwtPayload
        {
            {StandardClaimNames.Issuer, "issuer"},
            {StandardClaimNames.Subject, "issuer"},
            {StandardClaimNames.Audiences, "audience"},
            {StandardClaimNames.ExpirationTime, DateTimeOffset.UtcNow.AddDays(2).ConvertToUnixTimestamp()}
        };
        var jwks = CreateJwt(jwsPayload, out var jwt);
        var instruction = new AuthenticateInstruction
        {
            ClientAssertion = jwt //"invalid_header.invalid_payload"
        };
        var client = new Client {JsonWebKeys = jwks};

        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);

        var result = await _clientAssertionAuthentication
            .AuthenticateClientWithPrivateKeyJwt(instruction, "audience", CancellationToken.None)
            ;

        Assert.NotNull(result.Client);
    }

    [Fact]
    public async Task When_Decrypt_Valid_Client_Secret_Jwt_Then_Client_Is_Returned()
    {
        var jwsPayload = new JwtPayload
        {
            {StandardClaimNames.Issuer, "issuer"},
            {StandardClaimNames.Subject, "issuer"},
            {StandardClaimNames.Audiences, new[] {"audience"}},
            {StandardClaimNames.ExpirationTime, DateTime.Now.AddDays(2).ConvertToUnixTimestamp()}
        };

        var jwks = CreateJwt(jwsPayload, out var jwt);
        var instruction = new AuthenticateInstruction
        {
            ClientAssertion = jwt // "valid_header.valid.valid.valid.valid"
        };
        var client = new Client {JsonWebKeys = jwks};

        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);

        var result = await _clientAssertionAuthentication
            .AuthenticateClientWithClientSecretJwt(instruction, CancellationToken.None)
            ;

        Assert.NotNull(result);
    }
}
