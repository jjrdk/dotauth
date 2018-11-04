namespace SimpleIdentityServer.Core.Jwt.Converter
{
    using System.Collections.Generic;
    using Shared;
    using Shared.Requests;

    public interface IJsonWebKeyConverter
    {
        IEnumerable<JsonWebKey> ExtractSerializedKeys(JsonWebKeySet jsonWebKeySet);
    }
}