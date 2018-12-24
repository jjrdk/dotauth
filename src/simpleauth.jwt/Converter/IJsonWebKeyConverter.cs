namespace SimpleIdentityServer.Core.Jwt.Converter
{
    using System.Collections.Generic;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;

    public interface IJsonWebKeyConverter
    {
        IEnumerable<JsonWebKey> ExtractSerializedKeys(JsonWebKeySet jsonWebKeySet);
    }
}