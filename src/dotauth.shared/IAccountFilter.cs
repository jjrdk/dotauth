namespace DotAuth.Shared;

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the account filter interface.
/// </summary>
public interface IAccountFilter
{
    /// <summary>
    /// Checks the specified claims.
    /// </summary>
    /// <param name="claims">The claims.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<AccountFilterResult> Check(IEnumerable<Claim> claims, CancellationToken cancellationToken);
}