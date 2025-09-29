namespace DotAuth.Shared.Policies;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Defines the ticket line parameter content.
/// </summary>
public sealed record TicketLineParameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TicketLineParameter"/> class.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="scopes">The scopes.</param>
    /// <param name="isAuthorizedByRo">if set to <c>true</c> [is authorized by resource owner].</param>
    public TicketLineParameter(string clientId, IEnumerable<string>? scopes = null, bool isAuthorizedByRo = false)
    {
        ClientId = clientId;
        Scopes = scopes == null ? [] : scopes.ToArray();
        IsAuthorizedByRo = isAuthorizedByRo;
    }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    public string ClientId { get; }

    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    public string[] Scopes { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is authorized by ro.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is authorized by ro; otherwise, <c>false</c>.
    /// </value>
    public bool IsAuthorizedByRo { get; }
}