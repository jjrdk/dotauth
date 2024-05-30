namespace DotAuth.Uma.Client;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using DotAuth.Shared.Responses;

public static class Extensions
{
    public static async Task<byte[]> ToByteArray(this Task<Stream?> stream)
    {
        var s = await stream.ConfigureAwait(false);
        return await s.ToByteArray().ConfigureAwait(false);
    }

    public static async Task<byte[]> ToByteArray(this Stream? stream)
    {
        if (stream == null)
        {
            return [];
        }
        var buffer = new byte[stream.Length];
        var offset = 0;
        const int count = 4096;
        while (true)
        {
            var read = await stream
                .ReadAsync(buffer.AsMemory(offset, Math.Min(count, (int)stream.Length - offset)))
                .ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            offset += read;
        }

        return buffer;
    }

    public static string GetSubject(this GrantedTokenResponse tokenResponse)
    {
        return tokenResponse.IdToken.GetSubject();
    }

    public static string GetSubject(this string? idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return "";
        }
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(idToken);
        return jwt.Subject;
    }

    public static ClaimsPrincipal GetUncheckedPrincipal(this GrantedTokenResponse tokenResponse)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenResponse.AccessToken);
        return new ClaimsPrincipal(new ClaimsIdentity(jwt.Claims));
    }
}