namespace DotAuth.Server.Tests;

using DotAuth.Extensions;
using Microsoft.IdentityModel.Tokens;

public sealed class SharedUmaContext
{
    public JsonWebKey EncryptionKey { get; } = TestKeys.SecretKey.CreateEncryptionJwk();
    public JsonWebKey SignatureKey { get; } = TestKeys.SecretKey.CreateSignatureJwk();
}
