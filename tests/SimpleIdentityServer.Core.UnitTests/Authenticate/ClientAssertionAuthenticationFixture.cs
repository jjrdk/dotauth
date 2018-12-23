using Moq;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    using Shared;
    using Shared.Models;
    using Shared.Repositories;

    public sealed class ClientAssertionAuthenticationFixture
    {
        private Mock<IJwsParser> _jwsParserFake;
        private Mock<IClientStore> _clientRepositoryStub;
        private Mock<IJwtParser> _jwtParserFake;
        private IClientAssertionAuthentication _clientAssertionAuthentication;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_A_Not_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(false);

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
        }

        [Fact]
        public async Task When_A_Jws_Token_With_Not_Payload_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(() => null);

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
        }

        [Fact]
        public async Task When_A_Jws_Token_With_Invalid_Signature_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(() => Task.FromResult((JwsPayload)null));

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheSignatureIsNotCorrect);
        }

        [Fact]
        public async Task When_A_Jws_Token_With_Invalid_Issuer_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    StandardClaimNames.Issuer, "issuer"
                }
            };
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Client)null));

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientIdPassedInJwtIsNotCorrect);
        }

        [Fact]
        public async Task When_A_Jws_Token_With_Invalid_Audience_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    StandardClaimNames.Issuer, "issuer"
                },
                {
                    Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject, "issuer"
                },
                {
                    StandardClaimNames.Audiences, new []
                    {
                        "audience"
                    }
                }
            };
            var client = new Client();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, "invalid_issuer").ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheAudiencePassedInJwtIsNotCorrect);
        }

        [Fact]
        public async Task When_An_Expired_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    StandardClaimNames.Issuer, "issuer"
                },
                {
                    Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject, "issuer"
                },
                {
                    StandardClaimNames.Audiences, new []
                    {
                        "audience"
                    }
                },
                {
                    StandardClaimNames.ExpirationTime, DateTime.Now.AddDays(-2)
                }
            };
            var client = new Client();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, "audience").ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheReceivedJwtHasExpired);
        }

        [Fact]
        public async Task When_A_Valid_Jws_Token_Is_Passed_To_AuthenticateClientWithPrivateKeyJwt_Then_Client_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "invalid_header.invalid_payload"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    StandardClaimNames.Issuer, "issuer"
                },
                {
                    Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject, "issuer"
                },
                {
                    StandardClaimNames.Audiences, new []
                    {
                        "audience"
                    }
                },
                {
                    StandardClaimNames.ExpirationTime, DateTime.UtcNow.AddDays(2).ConvertToUnixTimestamp()
                }
            };
            var client = new Client();
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwsParserFake.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithPrivateKeyJwtAsync(instruction, "audience").ConfigureAwait(false);

                        Assert.NotNull(result.Client);
        }

        [Fact]
        public async Task When_Passing_Null_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(null, string.Empty, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_A_Not_Jwe_Token_To_AuthenticateClientWithClientSecretJwt_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(false);

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientAssertionIsNotAJweToken);
        }

        [Fact]
        public async Task When_Passing_A_Not_Valid_Jwe_Token_To_AuthenticateClientWithClientSecretJwt_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPasswordAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(string.Empty));

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheJweTokenCannotBeDecrypted);
        }

        [Fact]
        public async Task When_Decrypt_Client_Secret_Jwt_And_Its_Not_A_Jws_Token_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPasswordAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult("jws"));
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(false);

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
        }

        [Fact]
        public async Task When_Decrypt_Client_Secret_Jwt_And_Cannot_Extract_Jws_PayLoad_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPasswordAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult("jws"));
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult((JwsPayload)null));

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty, null).ConfigureAwait(false);

                        Assert.Null(result.Client);
            Assert.True(result.ErrorMessage == ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
        }

        [Fact]
        public async Task When_Decrypt_Valid_Client_Secret_Jwt_Then_Client_Is_Returned()
        {            InitializeFakeObjects();
            var instruction = new AuthenticateInstruction
            {
                ClientAssertion = "valid_header.valid.valid.valid.valid"
            };
            var jwsPayload = new JwsPayload
            {
                {
                    StandardClaimNames.Issuer, "issuer"
                },
                {
                    Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject, "issuer"
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
            var client = new Client();
            _jwtParserFake.Setup(j => j.IsJweToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.DecryptWithPasswordAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult("jws"));
            _jwtParserFake.Setup(j => j.IsJwsToken(It.IsAny<string>()))
                .Returns(true);
            _jwtParserFake.Setup(j => j.UnSignAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(jwsPayload));
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));

                        var result = await _clientAssertionAuthentication.AuthenticateClientWithClientSecretJwtAsync(instruction, string.Empty, "audience").ConfigureAwait(false);

                        Assert.NotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _jwsParserFake = new Mock<IJwsParser>();
            _clientRepositoryStub = new Mock<IClientStore>();
            _jwtParserFake = new Mock<IJwtParser>();
            _clientAssertionAuthentication = new ClientAssertionAuthentication(
                _jwsParserFake.Object,
                _clientRepositoryStub.Object,
                _jwtParserFake.Object);
        }
    }
}
