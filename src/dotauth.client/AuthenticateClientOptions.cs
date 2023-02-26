namespace DotAuth.Client;

using System;

public record AuthenticateClientOptions
{
    public required string ClientId { get; init; }

    public required string ClientSecret { get; init; }

    public required Uri Authority { get; init; }

    public required Uri Callback { get; init; }

    public required string[] Scopes { get; init; }
}