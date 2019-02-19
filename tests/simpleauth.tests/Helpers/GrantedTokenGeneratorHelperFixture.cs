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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using Xunit;

    public class GrantedTokenGeneratorHelperFixture
    {
        private readonly Mock<IClientStore> _clientRepositoryStub;

        public GrantedTokenGeneratorHelperFixture()
        {
            _clientRepositoryStub = new Mock<IClientStore>();
        }

        [Fact]
        public async Task When_Passing_NullOrWhiteSpace_Then_Exceptions_Are_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _clientRepositoryStub.Object.GenerateToken(
                        string.Empty,
                        null,
                        null,
                        CancellationToken.None,
                        null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_DoesNot_Exist_Then_Exception_Is_Thrown()
        {
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client) null);

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _clientRepositoryStub.Object.GenerateToken(
                        "invalid_client",
                        null,
                        null,
                        CancellationToken.None,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, ex.Code);
            Assert.Equal(ErrorDescriptions.TheClientIdDoesntExist, ex.Message);
        }

        [Fact]
        public async Task When_ExpirationTime_Is_Set_Then_ExpiresInProperty_Is_Set()
        {
            var client = new Client
            {
                TokenLifetime = TimeSpan.FromSeconds(3700),
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                ClientId = "client_id",
                IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256,
                IdTokenEncryptedResponseAlg = SecurityAlgorithms.RsaSha256,
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };

            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var result = await _clientRepositoryStub.Object
                .GenerateToken("client_id", "scope", "issuer", CancellationToken.None, null)
                .ConfigureAwait(false);

            Assert.Equal(3700, result.ExpiresIn);
        }
    }
}
