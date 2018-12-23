using SimpleIdentityServer.Core.Extensions;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Extensions
{
    using Shared;
    using Shared.Models;

    public sealed class ClientExtensionsFixture
    {
        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenSignedResponseAlg_Then_Null_Is_Returned()
        {            var client = new Client
            {
                IdTokenSignedResponseAlg = "not_supported"
            };

                        var result = client.GetIdTokenSignedResponseAlg();

                        Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetIdTokenSignedResponseAlg_Then_RS256_Is_Returned()
        {            var client = new Client
            {
                IdTokenSignedResponseAlg = Jwt.JwtConstants.JwsAlgNames.RS256
            };

                        var result = client.GetIdTokenSignedResponseAlg();

                        Assert.Equal(JwsAlg.RS256, result);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenEncryptedResponseAlg_Then_Null_Is_Returned()
        {            var client = new Client
            {
                IdTokenEncryptedResponseAlg = "not_supported"
            };

                        var result = client.GetIdTokenEncryptedResponseAlg();

                        Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetIdTokenEncryptedResponseAlg_Then_RSA1_5_Is_Returned()
        {            var client = new Client
            {
                IdTokenEncryptedResponseAlg = Jwt.JwtConstants.JweAlgNames.RSA1_5
            };

                        var result = client.GetIdTokenEncryptedResponseAlg();

                        Assert.True(result == JweAlg.RSA1_5);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenEncryptedResponseEnc_Then_Null_Is_Returned()
        {            var client = new Client
            {
                IdTokenEncryptedResponseEnc = "not_supported"
            };

                        var result = client.GetIdTokenEncryptedResponseEnc();

                        Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetIdTokenEncryptedResponseEnc_Then_A128CBC_HS256_Is_Returned()
        {            var client = new Client
            {
                IdTokenEncryptedResponseEnc = Jwt.JwtConstants.JweEncNames.A128CBC_HS256
            };

                        var result = client.GetIdTokenEncryptedResponseEnc();

                        Assert.True(result == JweEnc.A128CBC_HS256);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoSignedResponseAlg_Then_Null_Is_Returned()
        {            var client = new Client
            {
                UserInfoSignedResponseAlg = "not_supported"
            };

                        var result = client.GetUserInfoSignedResponseAlg();

                        Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetUserInfoSignedResponseAlg_Then_RS256_Is_Returned()
        {            var client = new Client
            {
                UserInfoSignedResponseAlg = Jwt.JwtConstants.JwsAlgNames.RS256
            };

                        var result = client.GetUserInfoSignedResponseAlg();

                        Assert.True(result == JwsAlg.RS256);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoEncryptedResponseAlg_Then_Null_Is_Returned()
        {            var client = new Client
            {
                UserInfoEncryptedResponseAlg = "not_supported"
            };

                        var result = client.GetUserInfoEncryptedResponseAlg();

                        Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetUserInfoEncryptedResponseAlg_Then_RSA1_5_Is_Returned()
        {            var client = new Client
            {
                UserInfoEncryptedResponseAlg = Jwt.JwtConstants.JweAlgNames.RSA1_5
            };

                        var result = client.GetUserInfoEncryptedResponseAlg();

                        Assert.True(result == JweAlg.RSA1_5);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoEncryptedResponseEnc_Then_Null_Is_Returned()
        {            var client = new Client
            {
                UserInfoEncryptedResponseEnc = "not_supported"
            };

                        var result = client.GetUserInfoEncryptedResponseEnc();

                        Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetUserInfoEncryptedResponseEnc_Then_A128CBC_HS256_Is_Returned()
        {            var client = new Client
            {
                UserInfoEncryptedResponseEnc = Jwt.JwtConstants.JweEncNames.A128CBC_HS256
            };

                        var result = client.GetUserInfoEncryptedResponseEnc();

                        Assert.True(result == JweEnc.A128CBC_HS256);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectSigningAlg_Then_Null_Is_Returned()
        {            var client = new Client
            {
                RequestObjectSigningAlg = "not_supported"
            };

                        var result = client.GetRequestObjectSigningAlg();

                        Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetRequestObjectSigningAlg_Then_RS256_Is_Returned()
        {            var client = new Client
            {
                RequestObjectSigningAlg = Jwt.JwtConstants.JwsAlgNames.RS256
            };

                        var result = client.GetRequestObjectSigningAlg();

                        Assert.True(result == JwsAlg.RS256);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectEncryptionAlg_Then_Null_Is_Returned()
        {            var client = new Client
            {
                RequestObjectEncryptionAlg = "not_supported"
            };

                        var result = client.GetRequestObjectEncryptionAlg();

                        Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetRequestObjectEncryptionAlg_Then_RSA1_5_Is_Returned()
        {            var client = new Client
            {
                RequestObjectEncryptionAlg = Jwt.JwtConstants.JweAlgNames.RSA1_5
            };

                        var result = client.GetRequestObjectEncryptionAlg();

                        Assert.True(result == JweAlg.RSA1_5);
        }

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectEncryptionEnc_Then_Null_Is_Returned()
        {            var client = new Client
            {
                RequestObjectEncryptionEnc = "not_supported"
            };

                        var result = client.GetRequestObjectEncryptionEnc();

                        Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetRequestObjectEncryptionEnc_Then_A128CBC_HS256_Is_Returned()
        {            var client = new Client
            {
                RequestObjectEncryptionEnc = Jwt.JwtConstants.JweEncNames.A128CBC_HS256
            };

                        var result = client.GetRequestObjectEncryptionEnc();

                        Assert.True(result == JweEnc.A128CBC_HS256);
        }
    }
}
