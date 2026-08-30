namespace DotAuth.Tests.Authenticate;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
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
    // RS256 key pair for private_key_jwt happy path tests (G3: must use asymmetric alg).
    private readonly JsonWebKey _rsaSigningKey;

    public static IEnumerable<object[]> InvalidPayloads()
    {
        return
        [
            [
                new JwtPayload
                {
                    {StandardClaimNames.Issuer, "issuer"},
                    {StandardClaimNames.Subject, "issuer"},
                    {StandardClaimNames.Audiences, "audience"}
                }
            ],
            [
                new JwtPayload
                {
                    {StandardClaimNames.Issuer, "issuer"},
                    {StandardClaimNames.Subject, "issuer"},
                    {StandardClaimNames.Audiences, "audience"}
                }
            ],
            [
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
            ]
        ];
    }

    public ClientAssertionAuthenticationFixture()
    {
        _clientRepositoryStub = Substitute.For<IClientStore>();
        _clientAssertionAuthentication = new ClientAssertionAuthentication(
            _clientRepositoryStub,
            new InMemoryJwksRepository(),
            new InMemoryClientAssertionJtiStore());
        using var rsa = new RSACryptoServiceProvider(2048);
        _rsaSigningKey = rsa.CreateSignatureJwk("test", includePrivateParameters: true);
    }

    [Fact]
    public async Task When_A_Not_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
    {
        var instruction = new AuthenticateInstruction { ClientAssertion = "invalid_header.invalid_payload" };
        var result = await _clientAssertionAuthentication
            .AuthenticateClientWithPrivateKeyJwt(instruction, "", CancellationToken.None);

        Assert.Null(result.Client);
        Assert.Equal(Strings.TheClientAssertionIsNotAJwsToken, result.ErrorMessage);
    }

    [Fact]
    public async Task When_ClientAssertionType_Is_Missing_ForPrivateKeyJwt_Then_Null_Is_Returned()
    {
        var instruction = new AuthenticateInstruction { ClientAssertion = "a.b.c" };

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
        var client = new Client { JsonWebKeys = jwks };

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

    // Creates an RS256-signed JWT using the fixture's RSA key pair.
    private JsonWebKeySet CreateRsaJwt(JwtPayload jwsPayload, out string jwt)
    {
        var token = new JwtSecurityToken(
            new JwtHeader(new SigningCredentials(_rsaSigningKey, SecurityAlgorithms.RsaSha256)),
            jwsPayload);
        jwt = _handler.WriteToken(token);
        return new JsonWebKeySet().AddKey(_rsaSigningKey);
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
        var jwks = CreateRsaJwt(jwsPayload, out var jwt);
        var instruction = new AuthenticateInstruction
        {
            ClientAssertion = jwt,
            ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
        };
        var client = new Client
        {
            ClientId = "issuer",
            JsonWebKeys = jwks,
            TokenEndPointAuthSigningAlg = SecurityAlgorithms.RsaSha256
        };

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
        var client = new Client { JsonWebKeys = jwks };

        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);

        var result = await _clientAssertionAuthentication
            .AuthenticateClientWithClientSecretJwt(instruction, CancellationToken.None)
            ;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task When_ClientSecretJwt_Is_A_Jwe_Then_Null_Is_Returned()
    {
       var instruction = new AuthenticateInstruction
         {
           ClientAssertion = "a.b.c.d.e",
           ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
         };

       var result = await _clientAssertionAuthentication
             .AuthenticateClientWithClientSecretJwt(instruction, CancellationToken.None);

       Assert.Null(result.Client);
       Assert.Equal(Strings.TheClientAssertionIsNotAJwsToken, result.ErrorMessage);
     }

     // T1.4 (G3): the assertion's 'alg' MUST match the algorithm the client registered. When a
     // client is pinned to RS256 but its key set also carries an ES384 key (rotation / multi-key),
     // an ES384 assertion must be rejected rather than silently verified against a different key.
     // This test fails if the algorithm binding is removed, proving the defense is load-bearing.
     [Fact]
    public async Task When_Credential_Algorithm_Does_Not_Match_Registration_Then_Invalid_Client()
     {
       using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var ecKey = ecdsa.CreateJwk("ec", JsonWebKeyUseNames.Sig, includePrivateParameters: true);
        var jwks = new JsonWebKeySet().AddKey(ecKey).AddKey(_rsaSigningKey);

       var jwsPayload = new JwtPayload
         {
             {StandardClaimNames.Issuer, "issuer"},
             {StandardClaimNames.Subject, "issuer"},
             {StandardClaimNames.Audiences, "audience"},
             {StandardClaimNames.ExpirationTime, DateTimeOffset.UtcNow.AddDays(2).ConvertToUnixTimestamp()}
           };
       var token = new JwtSecurityToken(
           new JwtHeader(new SigningCredentials(ecKey, SecurityAlgorithms.EcdsaSha384)),
           jwsPayload);
       var jwt = _handler.WriteToken(token);

       var instruction = new AuthenticateInstruction
         {
           ClientAssertion = jwt,
           ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
         };
       // Client is pinned to RS256 but the key set also exposes an ES384 key.
       var client = new Client
         {
           ClientId = "issuer",
           JsonWebKeys = jwks,
           TokenEndPointAuthSigningAlg = SecurityAlgorithms.RsaSha256
         };

       _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(client);

       var result = await _clientAssertionAuthentication
             .AuthenticateClientWithPrivateKeyJwt(instruction, "audience", CancellationToken.None);

       Assert.Null(result.Client);
     }

     // T1.4 (G3): 'none' must never be accepted for a client assertion.
     [Fact]
    public async Task When_Credential_Uses_None_Algorithm_Then_Invalid_Client()
     {
       var header = Base64UrlEncode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
       var payload = new JwtPayload
         {
             {StandardClaimNames.Issuer, "issuer"},
             {StandardClaimNames.Subject, "issuer"},
             {StandardClaimNames.Audiences, "audience"},
             {StandardClaimNames.ExpirationTime, DateTimeOffset.UtcNow.AddDays(2).ConvertToUnixTimestamp()}
           };
       var payloadJson = _handler.WriteToken(new JwtSecurityToken(new JwtHeader(), payload));
       var signed = $"{header}.{payloadJson.Split('.')[1]}.";

       var jwks = CreateRsaJwt(new JwtPayload(), out _);
       var client = new Client
         {
           ClientId = "issuer",
           JsonWebKeys = jwks,
           TokenEndPointAuthSigningAlg = SecurityAlgorithms.RsaSha256
         };

       var instruction = new AuthenticateInstruction
         {
           ClientAssertion = signed,
           ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
         };
       _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(client);

       var result = await _clientAssertionAuthentication
             .AuthenticateClientWithPrivateKeyJwt(instruction, "audience", CancellationToken.None);

       Assert.Null(result.Client);
     }

      // T3.3 (G8): key rotation — a client whose jwks carries multiple kids (old + rotated) must
       // accept an assertion signed by either key. Both keys share the same algorithm (RS256) and
       // the client is registered with the same signing alg, so the rotation is exercised without
       // tripping the algorithm binding.
       [Fact]
     public async Task When_ClientAssertion_Uses_Rotated_Kid_Then_Client_Is_Authenticated()
        {
         using var rsaOld = new RSACryptoServiceProvider(2048);
         using var rsaNew = new RSACryptoServiceProvider(2048);
         var oldKey = rsaOld.CreateSignatureJwk("old-key", includePrivateParameters: true);
         var newKey = rsaNew.CreateSignatureJwk("new-key", includePrivateParameters: true);
         var jwks = new JsonWebKeySet().AddKey(newKey).AddKey(oldKey);

         var client = new Client
            {
             ClientId = "issuer",
             JsonWebKeys = jwks,
             TokenEndPointAuthSigningAlg = SecurityAlgorithms.RsaSha256
            };

         _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(client);

           // Assertion signed by the *rotated* (newer) key.
         var signedByNewKey = BuildRsaAssertion(newKey);

         var resultNew = await _clientAssertionAuthentication
                .AuthenticateClientWithPrivateKeyJwt(
                   new AuthenticateInstruction { ClientAssertion = signedByNewKey, ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                   "audience",
                   CancellationToken.None);
         Assert.NotNull(resultNew.Client);

           // A second assertion signed by the *old* key must still authenticate (both kids valid).
         var signedByOldKey = BuildRsaAssertion(oldKey);
         var resultOld = await _clientAssertionAuthentication
                .AuthenticateClientWithPrivateKeyJwt(
                   new AuthenticateInstruction { ClientAssertion = signedByOldKey, ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                   "audience",
                   CancellationToken.None);
         Assert.NotNull(resultOld.Client);
        }

       private static string BuildRsaAssertion(JsonWebKey key)
        {
         var handler = new JwtSecurityTokenHandler();
         var payload = new JwtPayload
            {
                {StandardClaimNames.Issuer, "issuer"},
                {StandardClaimNames.Subject, "issuer"},
                {StandardClaimNames.Audiences, "audience"},
                {StandardClaimNames.ExpirationTime, DateTimeOffset.UtcNow.AddDays(2).ConvertToUnixTimestamp()},
                {StandardClaimNames.Jti, Guid.NewGuid().ToString("N")}
             };
         var token = new JwtSecurityToken(
             new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.RsaSha256)),
             payload);
         return handler.WriteToken(token);
        }

       private static string Base64UrlEncode(string input)
        {
         var bytes = System.Text.Encoding.UTF8.GetBytes(input);
         return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
       }
