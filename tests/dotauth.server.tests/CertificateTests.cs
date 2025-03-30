namespace DotAuth.Server.Tests;

using System.Security.Cryptography.X509Certificates;
using DotAuth.Extensions;
using DotAuth.Shared;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public sealed class CertificateTests
{
    [Fact]
    public void CanCreateJwk()
    {
        var jwk = X509CertificateLoader.LoadPkcs12FromFile(
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
