namespace SimpleAuth.Stores.Marten
{
    using System;
    using Microsoft.IdentityModel.Tokens;

    public class JsonWebKeyContainer
    {
        public string Id { get; set; }
        public JsonWebKey Jwk { get; set; }

        public static JsonWebKeyContainer Create(JsonWebKey key)
        {
            return new JsonWebKeyContainer
            {
                Id = Guid.NewGuid().ToString("N"),
                Jwk = key
            };
        }
    }
}