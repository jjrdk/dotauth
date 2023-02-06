namespace DotAuth.Stores.Marten;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using global::Marten;

/// <summary>
/// Defines the Marten based token store.
/// </summary>
/// <seealso cref="ITokenStore" />
public sealed class MartenTokenStore : ITokenStore
{
    private readonly Func<IDocumentSession> _sessionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenTokenStore"/> class.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    public MartenTokenStore(Func<IDocumentSession> sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    /// <inheritdoc />
    public async Task<GrantedToken?> GetToken(
        string scopes,
        string clientId,
        JwtPayload? idTokenJwsPayload = null,
        JwtPayload? userInfoJwsPayload = null,
        CancellationToken cancellationToken = default)
    {
        if (idTokenJwsPayload == null || userInfoJwsPayload == null)
        {
            return null;
        }

        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var options = await session.Query<GrantedToken>()
            .Where(
                x => x.ClientId == clientId
                     && x.Scope == scopes)
            .ToListAsync(token: cancellationToken)
            .ConfigureAwait(false);
        return options!.FirstOrDefault(x =>
            idTokenJwsPayload.All(y => x.IdTokenPayLoad?.Contains(y) == true) &&
            userInfoJwsPayload.All(y => x.UserInfoPayLoad?.Contains(y) == true));
    }

    /// <inheritdoc />
    public async Task<GrantedToken?> GetRefreshToken(string getRefreshToken, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var grantedToken = await session.Query<GrantedToken>()
            .FirstOrDefaultAsync(x => x.RefreshToken == getRefreshToken, token: cancellationToken)
            .ConfigureAwait(false);
        return grantedToken;
    }

    /// <inheritdoc />
    public async Task<GrantedToken?> GetAccessToken(string accessToken, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var grantedToken = await session.Query<GrantedToken>()
            .FirstOrDefaultAsync(x => x.AccessToken == accessToken, token: cancellationToken)
            .ConfigureAwait(false);
        return grantedToken;
    }

    /// <inheritdoc />
    public async Task<bool> AddToken(GrantedToken grantedToken, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.Store(grantedToken);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAccessToken(string accessToken, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.DeleteWhere<GrantedToken>(x => x.AccessToken == accessToken);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.DeleteWhere<GrantedToken>(x => x.RefreshToken == refreshToken);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}