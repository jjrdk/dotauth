namespace DotAuth.Services;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;

/// <summary>
/// Defines the default subject builder type.
/// </summary>
public sealed class DefaultSubjectBuilder : ISubjectBuilder
{
    /// <summary>
    /// Builds a subject identifier based on the passed claims.
    /// </summary>
    /// <param name="claims">The claims information.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>A subject as a <see cref="string"/>.</returns>
    public Task<string> BuildSubject(IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
        var subject = claims.GetSubject() ?? claims.GetEmail();
        if (subject == null)
        {
            return Task.FromResult(Id.Create());
        }

        var sha = SHA256.HashData(Encoding.UTF8.GetBytes(subject));
        var id = BitConverter.ToString(sha).Replace("-", "");
        return Task.FromResult(id);
    }
}