namespace DotAuth.Server.Tests;

using DotAuth.Extensions;
using Microsoft.IdentityModel.Tokens;

public sealed class SharedUmaContext
{
    public SharedUmaContext()
    {
        SignatureKey = TestKeys.SecretKey.CreateSignatureJwk();
        EncryptionKey = TestKeys.SecretKey.CreateEncryptionJwk();
    }

    public JsonWebKey EncryptionKey { get; }
    public JsonWebKey SignatureKey { get; }
}
