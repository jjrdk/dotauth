namespace DotAuth.Uma;
using System.Text.RegularExpressions;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the UMA ticket info class.
/// </summary>
public partial record UmaTicketInfo : ResourceResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmaTicketInfo"/> class.
    /// </summary>
    /// <param name="ticketId">The ticket id.</param>
    /// <param name="umaAuthority">The UMA authority host address.</param>
    /// <param name="realm">The application realm.</param>
    public UmaTicketInfo(
        string ticketId,
        [StringSyntax(StringSyntaxAttribute.Uri)] string umaAuthority,
        string? realm = null)
    {
        TicketId = ticketId;
        UmaAuthority = umaAuthority;
        Realm = realm;
    }

    /// <summary>
    /// Gets the ticket id.
    /// </summary>
    [JsonPropertyName("ticket_id")]
    public string TicketId { get; }

    /// <summary>
    /// Gets the UMA authority.
    /// </summary>
    [JsonPropertyName("uma_authority")]
    public string UmaAuthority { get; }

    /// <summary>
    /// Gets the application realm.
    /// </summary>
    [JsonPropertyName("realm")]
    public string? Realm { get; }

    /// <summary>
    /// Tries to parse the incoming header
    /// </summary>
    /// <param name="header">The header to parse.</param>
    /// <param name="info">The resulting <see cref="UmaTicketInfo"/> if successful</param>
    /// <returns><c>True</c> if parse is successful, otherwise <c>False</c>.</returns>
    public static bool TryParse(string header, out UmaTicketInfo? info)
    {
        var regex = ParseRegex();
        var match = regex.Match(header);
        if (match.Success)
        {
            info = new UmaTicketInfo(match.Groups["ticketId"].Value, match.Groups["umaAuthority"].Value, match.Groups["realm"].Value);
            return true;
        }

        info = null;
        return false;
    }

    [GeneratedRegex("as_uri=\"(?<umaAuthority>.+)\", ticket=\"(?<ticketId>.+)\"( realm=\"(?<realm>.+)\")?", RegexOptions.Compiled)]
    private static partial Regex ParseRegex();
}

