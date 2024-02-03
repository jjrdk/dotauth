namespace DotAuth.Uma;

using System.Runtime.Serialization;

[DataContract]
public record UmaRequestSubmitted : ResourceResult
{
    public UmaRequestSubmitted(string ticketId)
    {
        TicketId = ticketId;
    }

    /// <summary>
    /// Gets the ticket id for the request submission.
    /// </summary>
    [DataMember(Name = "ticket_id")]
    public string TicketId { get; }
}