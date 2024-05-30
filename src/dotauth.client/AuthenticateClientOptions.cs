namespace DotAuth.Client;

using System;

/// <summary>
/// Defines the options for the <see cref="AuthenticateClientBase"/>.
/// </summary>
#if NET7_0
public record AuthenticateClientOptions
{
    /// <summary>
    /// Gets or sets the client id of the application to authenticate.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets or sets the client secret of the application to authenticate.
    /// </summary>
    public required string ClientSecret { get; init; }

    /// <summary>
    /// Gets or sets the token authority of the application to authenticate.
    /// </summary>
    public required Uri Authority { get; init; }

    /// <summary>
    /// Gets or sets the callback <see cref="Uri"/> of the application to authenticate.
    /// </summary>
    public required Uri Callback { get; init; }

    /// <summary>
    /// Gets or sets the scopes to authenticate for.
    /// </summary>
    public required string[] Scopes { get; init; }

    /// <summary>
    /// Gets or sets the scopes to authenticate for.
    /// </summary>
    public string ResponseMode { get; init; } = "query";
}
#else
public record AuthenticateClientOptions
{
    /// <summary>
    /// Gets or sets the client id of the application to authenticate.
    /// </summary>
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the client secret of the application to authenticate.
    /// </summary>
    public string ClientSecret { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token authority of the application to authenticate.
    /// </summary>
    public Uri Authority { get; set; } = null!;

    /// <summary>
    /// Gets or sets the callback <see cref="Uri"/> of the application to authenticate.
    /// </summary>
    public Uri Callback { get; set; } = null!;

    /// <summary>
    /// Gets or sets the scopes to authenticate for.
    /// </summary>
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets the scopes to authenticate for.
    /// </summary>
    public string ResponseMode { get; set; } = "query";
}
#endif