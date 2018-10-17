namespace SimpleIdentityServer.Core.Jwt.Converter
{
    using System.Collections.Generic;
    using Common;
    using Common.DTOs.Requests;

    public interface IJsonWebKeyConverter
    {
        IEnumerable<JsonWebKey> ExtractSerializedKeys(JsonWebKeySet jsonWebKeySet);
    }
}