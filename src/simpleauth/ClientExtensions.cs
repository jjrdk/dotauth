namespace SimpleAuth
{
    using Microsoft.IdentityModel.Tokens;
    using Shared.Models;

    public static class ClientExtensions
    {
        public static TokenValidationParameters CreateValidationParameters(this Client client, string audience = null, string issuer = null)
        {
            var parameters = new TokenValidationParameters
            {
                IssuerSigningKeys = client.JsonWebKeys.GetSignKeys(),
                TokenDecryptionKeys = client.JsonWebKeys.GetEncryptionKeys()
            };
            if (audience != null)
            {
                parameters.ValidAudience = audience;
            }
            else
            {
                parameters.ValidateAudience = false;
            }
            if (issuer != null)
            {
                parameters.ValidIssuer = issuer;
            }
            else
            {
                parameters.ValidateIssuer = false;
            }

            return parameters;
        }
    }
}