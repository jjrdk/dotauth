namespace SimpleAuth.Tests.Authenticate
{
    using Errors;
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Authenticate;
    using SimpleAuth.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class ClientAssertionAuthenticationFixture
    {
        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();
        private Mock<IClientStore> _clientRepositoryStub;
        private ClientAssertionAuthentication _clientAssertionAuthentication;

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
                        {StandardClaimNames.ExpirationTime, DateTime.Now.AddDays(-2).ConvertToUnixTimestamp()}
                    }
                }
            };
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_A_Not_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {
            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            //_jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
            //    .Returns(false);

            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, null).ConfigureAwait(false);

            Assert.Null(result.Client);
            Assert.Equal(ErrorDescriptions.TheClientAssertionIsNotAJwsToken, result.ErrorMessage);
        }

        [Theory]
        [MemberData(nameof(InvalidPayloads))]
        public async Task WhenInvalidJwtIsPassedThenReturnsNullClient(JwtPayload jwsPayload)
        {
            InitializeFakeObjects();
            var jwks = CreateJwt(jwsPayload, out var jwt);
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = jwt // "invalid_header.invalid_payload"
            };
            var client = new Client
            {
                JsonWebKeys = jwks
            };

            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, "invalid_issuer").ConfigureAwait(false);

            Assert.Null(result.Client);
            Assert.NotNull(result.ErrorMessage);
        }

        //[Fact]
        //public async Task When_A_Jws_Token_With_Not_Payload_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        //{
        //    InitializeFakeObjects();
        //    var instruction = new AuthenticateInstruction
        //    {
        //        ClientAssertion = "invalid_header.invalid_payload"
        //    };
        //    //_jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
        //    //    .Returns(true);
        //    //_jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
        //    //    .Returns(() => null);

        //    var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, null).ConfigureAwait(false);

        //    Assert.Null(result.Client);
        //    Assert.Equal(ErrorDescriptions.TheJwsPayloadCannotBeExtracted, result.ErrorMessage);
        //}

        //[Fact]
        //public async Task When_A_Jws_Token_With_Invalid_Signature_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        //{
        //    InitializeFakeObjects();
        //    var instruction = new AuthenticateInstruction
        //    {
        //        ClientAssertion = "invalid_header.invalid_payload"
        //    };
        //    //var jwsPayload = new JwtSecurityToken();
        //    //_jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
        //    //    .Returns(true);
        //    //_jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
        //    //    .Returns(jwsPayload);
        //    //_jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
        //    //    It.IsAny<string>()))
        //    //    .Returns(() => Task.FromResult((JwtSecurityToken)null));

        //    var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, null).ConfigureAwait(false);

        //    Assert.Null(result.Client);
        //    Assert.Equal(ErrorDescriptions.TheSignatureIsNotCorrect, result.ErrorMessage);
        //}

        //[Fact]
        //public async Task When_A_Jws_Token_With_Invalid_Issuer_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        //{
        //    InitializeFakeObjects();
        //    var instruction = new AuthenticateInstruction
        //    {
        //        ClientAssertion = "invalid_header.invalid_payload"
        //    };
        //    //var jwsPayload = new JwtSecurityToken
        //    //{
        //    //    {
        //    //        StandardClaimNames.Issuer, "issuer"
        //    //    }
        //    //};
        //    //_jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
        //    //    .Returns(true);
        //    //_jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
        //    //    .Returns(jwsPayload);
        //    //_jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
        //    //    It.IsAny<string>()))
        //    //    .Returns(Task.FromResult(jwsPayload));
        //    _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
        //        .Returns(() => Task.FromResult((Client)null));

        //    var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, "wrong").ConfigureAwait(false);

        //    Assert.Null(result.Client);
        //    Assert.Equal(ErrorDescriptions.TheClientIdPassedInJwtIsNotCorrect, result.ErrorMessage);
        //}

        private JsonWebKeySet CreateJwt(JwtPayload jwsPayload, out string jwt)
        {
            var jwks = "verylongsecretkey".CreateSignatureJwk().ToSet();

            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(jwks.Keys[0], SecurityAlgorithms.HmacSha256)),
                jwsPayload);
            jwt = _handler.WriteToken(token);
            return jwks;
        }

        [Fact]
        public async Task When_A_Valid_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Client_Is_Returned()
        {
            InitializeFakeObjects();

            var jwsPayload = new JwtPayload
            {
                {
                    StandardClaimNames.Issuer, "issuer"
                },
                {
                    StandardClaimNames.Subject, "issuer"
                },
                {
                    StandardClaimNames.Audiences, "audience"
                },
                {
                    StandardClaimNames.ExpirationTime, DateTime.UtcNow.AddDays(2).ConvertToUnixTimestamp()
                }
            };
            var jwks = CreateJwt(jwsPayload, out var jwt);
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = jwt //"invalid_header.invalid_payload"
            };
            var client = new Client
            {
                JsonWebKeys = jwks
            };
            //_jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
            //    .Returns(true);
            //_jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
            //    .Returns(jwsPayload);
            //_jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
            //    It.IsAny<string>()))
            //    .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, "audience").ConfigureAwait(false);

            Assert.NotNull(result.Client);
        }

        [Fact]
        public async Task When_Passing_Null_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(null, string.Empty, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Decrypt_Valid_Client_Secret_Jwt_Then_Client_Is_Returned()
        {
            InitializeFakeObjects();
            var jwsPayload = new JwtPayload
            {
                {
                    StandardClaimNames.Issuer, "issuer"
                },
                {
                    StandardClaimNames.Subject, "issuer"
                },
                {
                    StandardClaimNames.Audiences, new []
                    {
                        "audience"
                    }
                },
                {
                    StandardClaimNames.ExpirationTime, DateTime.Now.AddDays(2).ConvertToUnixTimestamp()
                }
            };

            var jwks = CreateJwt(jwsPayload, out var jwt);
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = jwt// "valid_header.valid.valid.valid.valid"
            };
            var client = new Client
            {
                JsonWebKeys = jwks
            };
            //_jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
            //    .Returns(true);
            //_jwtParserFake.Setup(j => j.DecryptWithPasswordAsync(It.IsAny<string>(),
            //    It.IsAny<string>(),
            //    It.IsAny<string>()))
            //    .Returns(Task.FromResult("jws"));
            //_jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
            //    .Returns(true);
            //_jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(), It.IsAny<string>()))
            //    .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

            var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty, "audience").ConfigureAwait(false);

            Assert.NotNull(result);
        }

        private void InitializeFakeObjects()
        {
            //_jwsParserFake = new Mock<IJwsParser>();
            _clientRepositoryStub = new Mock<IClientStore>();
            //_jwtParserFake = new Mock<IJwtParser>();
            _clientAssertionAuthentication = new ClientAssertionAuthentication(_clientRepositoryStub.Object);
        }
    }
}
