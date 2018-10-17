namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    using System.Collections.Generic;
    using Common;

    public interface IJsonWebKeyEnricher
    {
        Dictionary<string, object> GetPublicKeyInformation(JsonWebKey jsonWebKey);
        Dictionary<string, object> GetJsonWebKeyInformation(JsonWebKey jsonWebKey);
    }
}