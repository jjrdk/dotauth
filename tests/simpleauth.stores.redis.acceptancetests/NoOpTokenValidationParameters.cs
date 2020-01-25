namespace SimpleAuth.Stores.Redis.AcceptanceTests
{
    using Microsoft.IdentityModel.Tokens;

    internal class NoOpTokenValidationParameters : TokenValidationParameters
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
}