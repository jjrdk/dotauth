namespace DotAuth.Uma;

public record ResourceRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceRequest"/> record.
    /// </summary>
    /// <param name="owner">The request owner subject</param>
    /// <param name="resourceId">The resource set id.</param>
    /// <param name="scope">The request scope.</param>
    /// <param name="ticketId">The ticket id.</param>
    public ResourceRequest(string owner, string resourceId, string[] scope, string ticketId)
    {
        Owner = owner;
        ResourceId = resourceId;
        Scope = scope;
        TicketId = ticketId;
    }

    /// <summary>
    /// Gets the request owner subject.
    /// </summary>
    public string Owner { get; }

    /// <summary>
    /// Gets the resource set id.
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// Gets the request scope.
    /// </summary>
    public string[] Scope { get; }

    /// <summary>
    /// Gets the ticket id.
    /// </summary>
    public string TicketId { get; }
}