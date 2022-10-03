namespace DotAuth.AcceptanceTests;

using Microsoft.IdentityModel.Tokens;

internal sealed class NoOpTokenValidationParameters : TokenValidationParameters
{
    public NoOpTokenValidationParameters(SharedContext context)
    {
        RequireExpirationTime = false;
        RequireSignedTokens = false;
        SaveSigninToken = false;
        ValidateActor = false;
        ValidateAudience = false;
        ValidateIssuer = false;
        ValidateIssuerSigningKey = true;
        ValidateLifetime = false;
        ValidateTokenReplay = false;
        IssuerSigningKey = context.SignatureKey;
    }
}