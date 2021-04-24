// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.Tests.Helpers
{
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Properties;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Properties;
    using Xunit;

    public class GrantedTokenGeneratorHelperFixture
    {
        private readonly Mock<IClientStore> _clientRepositoryStub;

        public GrantedTokenGeneratorHelperFixture()
        {
            _clientRepositoryStub = new Mock<IClientStore>();
        }

        [Fact]
        public async Task WhenPassingNullOrWhiteSpaceThenErrorIsReturned()
        {
            var result = await _clientRepositoryStub.Object.GenerateToken(
                          new InMemoryJwksRepository(),
                          string.Empty,
                          "",
                          "",
                          CancellationToken.None,
                          userInformationPayload: null)
                  .ConfigureAwait(false);

            Assert.IsType<Option<GrantedToken>.Error>(result);
        }

        [Fact]
        public async Task WhenClientDoesNotExistThenErrorIsReturned()
        {
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client)null);

            var ex = await _clientRepositoryStub.Object.GenerateToken(
                        new InMemoryJwksRepository(),
                        "invalid_client",
                        "",
                        "",
                        CancellationToken.None,
                        userInformationPayload: null)
                .ConfigureAwait(false) as Option<GrantedToken>.Error;
            Assert.Equal(ErrorCodes.InvalidClient, ex.Details.Title);
            Assert.Equal(SharedStrings.TheClientDoesntExist, ex.Details.Detail);
        }

        [Fact]
        public async Task When_ExpirationTime_Is_Set_Then_ExpiresInProperty_Is_Set()
        {
            var client = new Client
            {
                TokenLifetime = TimeSpan.FromSeconds(3700),
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                ClientId = "client_id",
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                IdTokenEncryptedResponseAlg = SecurityAlgorithms.RsaSha256,
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var result = await _clientRepositoryStub.Object.GenerateToken(
                    new InMemoryJwksRepository(),
                    "client_id",
                     "scope",
                    "issuer",
                    CancellationToken.None,
                    userInformationPayload: null)
                .ConfigureAwait(false) as Option<GrantedToken>.Result;

            Assert.Equal(3700, result.Item.ExpiresIn);
        }
    }
}
