namespace DotAuth.Parameters;

internal sealed record ClaimsParameter
{
    public ClaimParameter[] UserInfo { get; init; } = [];

    public ClaimParameter[] IdToken { get; init; } = [];
}