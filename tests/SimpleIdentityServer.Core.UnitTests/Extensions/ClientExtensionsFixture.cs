using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Common;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Extensions
{
    public sealed class ClientExtensionsFixture
    {
        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenSignedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenSignedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetIdTokenSignedResponseAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetIdTokenSignedResponseAlg_Then_RS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenSignedResponseAlg = Jwt.JwtConstants.JwsAlgNames.RS256
            };

            // ACT
            var result = client.GetIdTokenSignedResponseAlg();

            // ASSERT
            Assert.Equal(JwsAlg.RS256, result);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenEncryptedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenEncryptedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetIdTokenEncryptedResponseAlg_Then_RSA1_5_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenEncryptedResponseAlg = Jwt.JwtConstants.JweAlgNames.RSA1_5
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseAlg();

            // ASSERT
            Assert.True(result == JweAlg.RSA1_5);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenEncryptedResponseEnc_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenEncryptedResponseEnc = "not_supported"
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseEnc();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetIdTokenEncryptedResponseEnc_Then_A128CBC_HS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenEncryptedResponseEnc = Jwt.JwtConstants.JweEncNames.A128CBC_HS256
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseEnc();

            // ASSERT
            Assert.True(result == JweEnc.A128CBC_HS256);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoSignedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoSignedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetUserInfoSignedResponseAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetUserInfoSignedResponseAlg_Then_RS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoSignedResponseAlg = Jwt.JwtConstants.JwsAlgNames.RS256
            };

            // ACT
            var result = client.GetUserInfoSignedResponseAlg();

            // ASSERT
            Assert.True(result == JwsAlg.RS256);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoEncryptedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoEncryptedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetUserInfoEncryptedResponseAlg_Then_RSA1_5_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoEncryptedResponseAlg = Jwt.JwtConstants.JweAlgNames.RSA1_5
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseAlg();

            // ASSERT
            Assert.True(result == JweAlg.RSA1_5);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoEncryptedResponseEnc_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoEncryptedResponseEnc = "not_supported"
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseEnc();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetUserInfoEncryptedResponseEnc_Then_A128CBC_HS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoEncryptedResponseEnc = Jwt.JwtConstants.JweEncNames.A128CBC_HS256
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseEnc();

            // ASSERT
            Assert.True(result == JweEnc.A128CBC_HS256);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectSigningAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectSigningAlg = "not_supported"
            };

            // ACT
            var result = client.GetRequestObjectSigningAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetRequestObjectSigningAlg_Then_RS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectSigningAlg = Jwt.JwtConstants.JwsAlgNames.RS256
            };

            // ACT
            var result = client.GetRequestObjectSigningAlg();

            // ASSERT
            Assert.True(result == JwsAlg.RS256);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectEncryptionAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectEncryptionAlg = "not_supported"
            };

            // ACT
            var result = client.GetRequestObjectEncryptionAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetRequestObjectEncryptionAlg_Then_RSA1_5_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectEncryptionAlg = Jwt.JwtConstants.JweAlgNames.RSA1_5
            };

            // ACT
            var result = client.GetRequestObjectEncryptionAlg();

            // ASSERT
            Assert.True(result == JweAlg.RSA1_5);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectEncryptionEnc_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectEncryptionEnc = "not_supported"
            };

            // ACT
            var result = client.GetRequestObjectEncryptionEnc();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetRequestObjectEncryptionEnc_Then_A128CBC_HS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectEncryptionEnc = Jwt.JwtConstants.JweEncNames.A128CBC_HS256
            };

            // ACT
            var result = client.GetRequestObjectEncryptionEnc();

            // ASSERT
            Assert.True(result == JweEnc.A128CBC_HS256);
        }
    }
}
