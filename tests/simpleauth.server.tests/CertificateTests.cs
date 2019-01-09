namespace SimpleAuth.Server.Tests
{
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Xunit;

    public class CertificateTests
    {
        [Fact]
        public void CanExportPrivateKey()
        {
            var certificate = new X509Certificate2("mycert.pfx", "simpleauth", X509KeyStorageFlags.Exportable);
            using (var rsa = (RSA) certificate.PrivateKey)
            {
                var xml = RsaExtensions.ToXmlString(rsa, true);

                Assert.NotEmpty(xml);
            }
        }

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