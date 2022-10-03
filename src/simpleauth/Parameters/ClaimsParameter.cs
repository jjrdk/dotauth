namespace DotAuth.Parameters;

using System;

internal sealed record ClaimsParameter
{
    public ClaimParameter[] UserInfo { get; init; } = Array.Empty<ClaimParameter>();

    public ClaimParameter[] IdToken { get; init; } = Array.Empty<ClaimParameter>();
}