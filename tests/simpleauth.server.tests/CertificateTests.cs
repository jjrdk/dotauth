namespace SimpleAuth.Server.Tests
{
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using SimpleAuth.Extensions;
    using Xunit;

    public class CertificateTests
    {
        [Fact]
        public void CanCreateJwk()
        {
            var jwk = new X509Certificate2(
                    "mycert.pfx",
                    "simpleauth",
                    X509KeyStorageFlags.Exportable)
                .CreateJwk(
                    JsonWebKeyUseNames.Sig,
                    KeyOperations.Sign,
                    KeyOperations.Verify);

            Assert.True(jwk.HasPrivateKey);
        }
    }
}