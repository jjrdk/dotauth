namespace DotAuth.Client;

using System;

/// <summary>
/// Defines the options for the <see cref="AuthenticateClientBase"/>.
/// </summary>
public record AuthenticateClientOptions
{
    /// <summary>
    /// Gets or sets the client id of the application to authenticate.
    /// </summary>
    public required string ClientId
    {
        get;
#if NETSTANDARD2_1
        set;
#else
        init;
#endif
    }

    /// <summary>
    /// Gets or sets the client secret of the application to authenticate.
    /// </summary>
    public required string ClientSecret
    {
        get;
#if NETSTANDARD2_1
        set;
#else
        init;
#endif
    }

    /// <summary>
    /// Gets or sets the token authority of the application to authenticate.
    /// </summary>
    public required Uri Authority
    {
        get;
#if NETSTANDARD2_1
        set;
#else
        init;
#endif
    }

    /// <summary>
    /// Gets or sets the callback <see cref="Uri"/> of the application to authenticate.
    /// </summary>
    public required Uri Callback
    {
        get;
#if NETSTANDARD2_1
        set;
#else
        init;
#endif
    }

    /// <summary>
    /// Gets or sets the scopes to authenticate for.
    /// </summary>
    public required string[] Scopes
    {
        get;
#if NETSTANDARD2_1
        set;
#else
        init;
#endif
    }

    /// <summary>
    /// Gets or sets the scopes to authenticate for.
    /// </summary>
    public string ResponseMode
    {
        get;
#if NETSTANDARD2_1
        set;
#else
        init;
#endif
    } = "query";
}
