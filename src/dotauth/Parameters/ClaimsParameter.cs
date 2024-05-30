namespace DotAuth.Parameters;

using System;

internal sealed record ClaimsParameter
{
    public ClaimParameter[] UserInfo { get; init; } = [];

    public ClaimParameter[] IdToken { get; init; } = [];
}