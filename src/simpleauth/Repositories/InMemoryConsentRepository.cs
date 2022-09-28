namespace SimpleAuth.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shared.Models;
using Shared.Repositories;

/// <summary>
/// Defines the in-memory consent repository.
/// </summary>
/// <seealso cref="IConsentRepository" />
internal sealed class InMemoryConsentRepository : IConsentRepository
{
    private readonly ICollection<Consent> _consents;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryConsentRepository"/> class.
    /// </summary>
    /// <param name="consents">The consents.</param>
    public InMemoryConsentRepository(IReadOnlyCollection<Consent>? consents = null)
    {
        _consents = consents == null ? new List<Consent>() : consents.ToList();
    }

    /// <inheritdoc />
    public Task<bool> Delete(Consent record, CancellationToken cancellationToken = default)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        var consent = _consents.FirstOrDefault(c => c.Id == record.Id);
        if (consent == null)
        {
            return Task.FromResult(false);
        }

        _consents.Remove(consent);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Consent>> GetConsentsForGivenUser(
        string subject,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<Consent>>(
            _consents.Where(c => c.Subject == subject).ToList());
    }

    /// <inheritdoc />
    public Task<bool> Insert(Consent record, CancellationToken cancellationToken = default)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        _consents.Add(record);
        return Task.FromResult(true);
    }
}