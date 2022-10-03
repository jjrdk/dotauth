namespace DotAuth.JwtToken;

using System.Collections.Generic;
using System.Security.Claims;

internal sealed class ClaimEqualityComparer : IEqualityComparer<Claim>
{
    /// <inheritdoc />
    public bool Equals(Claim? x, Claim? y)
    {
        if (x == null)
        {
            return y == null;
        }

        return y != null && x.ToString().Equals(y.ToString());
    }

    /// <inheritdoc />
    public int GetHashCode(Claim obj)
    {
        return obj.ToString().GetHashCode();
    }
}