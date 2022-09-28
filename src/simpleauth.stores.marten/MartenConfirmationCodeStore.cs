﻿namespace SimpleAuth.Stores.Marten;

using System;
using System.Threading;
using System.Threading.Tasks;
using global::Marten;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Repositories;

/// <summary>
/// Defines the marten based confirmation code store.
/// </summary>
/// <seealso cref="SimpleAuth.Shared.Repositories.IConfirmationCodeStore" />
public sealed class MartenConfirmationCodeStore : IConfirmationCodeStore
{
    private readonly Func<IDocumentSession> _sessionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenScopeRepository"/> class.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    public MartenConfirmationCodeStore(Func<IDocumentSession> sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    /// <inheritdoc />
    public async Task<ConfirmationCode?> Get(string code, string subject, CancellationToken cancellationToken)
    {
        await using var session = _sessionFactory();
        var authorizationCode =
            await session.LoadAsync<ConfirmationCode>(code, cancellationToken).ConfigureAwait(false);

        return authorizationCode;
    }

    /// <inheritdoc />
    public async Task<bool> Add(ConfirmationCode confirmationCode, CancellationToken cancellationToken)
    {
        await using var session = _sessionFactory();
        session.Store(confirmationCode);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> Remove(string code, string subject, CancellationToken cancellationToken)
    {
        await using var session = _sessionFactory();
        session.Delete<ConfirmationCode>(code);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}