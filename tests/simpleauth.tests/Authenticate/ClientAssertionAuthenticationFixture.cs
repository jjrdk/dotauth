namespace SimpleAuth.Tests.Authenticate
{
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Authenticate;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Errors;
    using Xunit;

    public class ClientAssertionAuthenticationFixture
    {
        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();
        private readonly Mock<IClientStore> _clientRepositoryStub;
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
            _clientRepositoryStub = new Mock<IClientStore>();
            _clientAssertionAuthentication = new ClientAssertionAuthentication(_clientRepositoryStub.Object);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwt(
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_A_Not_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            var instruction = new AuthenticateInstruction {ClientAssertion = "invalid_header.invalid_payload"};
            var result = await _clientAssertionAuthentication
                .AuthenticateClientWithPrivateKeyJwt(instruction, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Null(result.Client);
            Assert.Equal(ErrorDescriptions.TheClientAssertionIsNotAJwsToken, result.ErrorMessage);
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

            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var result = await _clientAssertionAuthentication
                .AuthenticateClientWithPrivateKeyJwt(instruction, "invalid_issuer", CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Null(result.Client);
            Assert.NotNull(result.ErrorMessage);
        }

        private JsonWebKeySet CreateJwt(JwtPayload jwsPayload, out string jwt)
        {
            var jwks = "verylongsecretkey".CreateSignatureJwk().ToSet();

            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(jwks.Keys[0], SecurityAlgorithms.HmacSha256Signature)),
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
                {StandardClaimNames.ExpirationTime, DateTime.UtcNow.AddDays(2).ConvertToUnixTimestamp()}
            };
            var jwks = CreateJwt(jwsPayload, out var jwt);
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = jwt //"invalid_header.invalid_payload"
            };
            var client = new Client {JsonWebKeys = jwks};

            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var result = await _clientAssertionAuthentication
                .AuthenticateClientWithPrivateKeyJwt(instruction, "audience", CancellationToken.None)
                .ConfigureAwait(false);

            Assert.NotNull(result.Client);
        }

        [Fact]
        public async Task When_Passing_Null_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwt(
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
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

            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var result = await _clientAssertionAuthentication
                .AuthenticateClientWithClientSecretJwt(instruction, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.NotNull(result);
        }
    }
}
