namespace DotAuth.Uma;

using System.Text.Json.Serialization;

public record UmaRequestSubmitted : ResourceResult
{
    public UmaRequestSubmitted(string ticketId)
    {
        TicketId = ticketId;
    }

    /// <summary>
    /// Gets the ticket id for the request submission.
    /// </summary>
    [JsonPropertyName("ticket_id")]
    public string TicketId { get; }
}
