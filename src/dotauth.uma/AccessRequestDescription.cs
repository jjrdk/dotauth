namespace DotAuth.Uma;

using System;

public class ResourceAccessDescription
{
    public required string ResourceSetId { get; init; }

    public required string[] Scopes { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required Uri? IconUri { get; init; }

    public required string ResourceType { get; init; }
}

public class AccessRequestDescription
{
    public required string RequesterName { get; init; }

    public required string RequesterEmail { get; init; }

    public required ResourceAccessDescription[] RequestedResources { get; init; }

    public required string TicketId { get; init; }

    public required DateTimeOffset Created { get; init; }

    public required DateTimeOffset Expires { get; init; }
}
