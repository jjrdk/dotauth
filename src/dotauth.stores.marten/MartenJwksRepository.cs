namespace DotAuth.Stores.Marten;

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using DotAuth.Stores.Marten.Containers;
using global::Marten;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the marten based jwks repository.
/// </summary>
/// <seealso cref="IJwksRepository" />
public sealed class MartenJwksRepository : IJwksRepository
{
    private readonly Func<IDocumentSession> _sessionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenConsentRepository"/> class.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    public MartenJwksRepository(Func<IDocumentSession> sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    /// <inheritdoc />
    public async Task<JsonWebKeySet?> GetPublicKeys(CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var keysets = await session.Query<JsonWebKeyContainer>()
            .Where(x => x.Jwk.HasPrivateKey == false)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var jwks = keysets.Select(x => x.Jwk).ToSet();
        return jwks;
    }

    /// <inheritdoc />
    public async Task<SigningCredentials?> GetSigningKey(string alg, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var webKeys = await session.Query<JsonWebKeyContainer>()
            .Where(x => x.Jwk.HasPrivateKey == true && x.Jwk.Alg == alg && x.Jwk.Use == JsonWebKeyUseNames.Sig)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (webKeys.Count == 0)
        {
            return null;
        }

        var webKey = webKeys.First(x => x.Jwk.KeyOps.Contains(KeyOperations.Sign));

        if (webKey.Jwk.X5c != null)
        {
            foreach (var certString in webKey.Jwk.X5c)
            {
                return new X509SigningCredentials(
                    X509CertificateLoader.LoadCertificate(Convert.FromBase64String(certString)));
            }
        }

        return new SigningCredentials(webKey.Jwk, alg);
    }

    /// <inheritdoc />
    public async Task<SecurityKey?> GetEncryptionKey(string alg, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var webKeys = await session.Query<JsonWebKeyContainer>()
            .Where(
                x => x.Jwk.HasPrivateKey == true && x.Jwk.Alg == alg && x.Jwk.Use == JsonWebKeyUseNames.Enc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (webKeys.Count == 0)
        {
            return null;
        }

        var webKey = webKeys.First(x => x.Jwk.KeyOps.Contains(KeyOperations.Encrypt));

        if (webKey.Jwk.X5c == null)
        {
            return webKey.Jwk;
        }

        foreach (var certString in webKey.Jwk.X5c)
        {
            return new X509SecurityKey(X509CertificateLoader.LoadCertificate(Convert.FromBase64String(certString)));
        }

        return webKey.Jwk;
    }

    /// <inheritdoc />
    public async Task<SigningCredentials?> GetDefaultSigningKey(CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var webKeys = await session.Query<JsonWebKeyContainer>()
            .Where(x => x.Jwk.Use == JsonWebKeyUseNames.Sig)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (webKeys.Count == 0)
        {
            return null;
        }

        var webKey = webKeys.OrderBy(x => x.Jwk.KeyId).First(x => x.Jwk.KeyOps.Contains(KeyOperations.Sign));

        if (webKey.Jwk.X5c == null)
        {
            return new SigningCredentials(webKey.Jwk, webKey.Jwk.Alg);
        }

        foreach (var certString in webKey.Jwk.X5c)
        {
            return new X509SigningCredentials(
                X509CertificateLoader.LoadCertificate(Convert.FromBase64String(certString)));
        }

        return new SigningCredentials(webKey.Jwk, webKey.Jwk.Alg);
    }

    /// <inheritdoc />
    public async Task<bool> Add(JsonWebKey key, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var keyContainer = JsonWebKeyContainer.Create(key);
        session.Store(keyContainer);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> Rotate(JsonWebKeySet keySet, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        foreach (var key in keySet.Keys)
        {
            var keyKeyId = key.KeyId;
            session.DeleteWhere<JsonWebKeyContainer>(x => x.Jwk.KeyId == keyKeyId);
        }

        session.Store(keySet.Keys.Select(JsonWebKeyContainer.Create).ToArray());
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
