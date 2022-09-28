namespace SimpleAuth.Stores.Marten;

using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

internal static class JwkExtensions
{
    public static JsonWebKeySet ToSet(this IEnumerable<JsonWebKey> keys)
    {
        var jwks = new JsonWebKeySet();
        foreach (var key in keys)
        {
            jwks.Keys.Add(key);
        }

        return jwks;
    }
}