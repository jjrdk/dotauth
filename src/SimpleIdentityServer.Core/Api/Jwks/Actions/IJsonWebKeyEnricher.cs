namespace SimpleAuth.Api.Jwks.Actions
{
    using System.Collections.Generic;
    using Shared;

    public interface IJsonWebKeyEnricher
    {
        Dictionary<string, object> GetPublicKeyInformation(JsonWebKey jsonWebKey);
        Dictionary<string, object> GetJsonWebKeyInformation(JsonWebKey jsonWebKey);
    }
}