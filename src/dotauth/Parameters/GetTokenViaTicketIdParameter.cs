namespace DotAuth.Parameters;

internal sealed record GetTokenViaTicketIdParameter : GrantTypeParameter
{
    public string? Ticket { get; init; }
    public ClaimTokenParameter ClaimToken { get; init; } = null!;
    public string? Pct { get; init; }
    public string? Rpt { get; init; }
}