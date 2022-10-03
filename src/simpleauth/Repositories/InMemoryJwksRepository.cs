﻿namespace DotAuth.Repositories;

using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using Microsoft.IdentityModel.Tokens;

internal sealed class InMemoryJwksRepository : IJwksRepository
{
    private readonly JsonWebKeySet _privateKeySet;
    private readonly JsonWebKeySet _publicKeySet;

    public InMemoryJwksRepository(JsonWebKeySet publicKeySet, JsonWebKeySet privateKeySet)
    {
        _publicKeySet = publicKeySet;
        _privateKeySet = privateKeySet;
    }

    public InMemoryJwksRepository()
    {
        using var rsa = RSA.Create();
        var privateKeys = new[]
        {
            rsa.CreateJwk("1", JsonWebKeyUseNames.Sig, true, KeyOperations.Sign, KeyOperations.Verify),
            rsa.CreateJwk("2", JsonWebKeyUseNames.Enc, true, KeyOperations.Encrypt, KeyOperations.Decrypt)
        };
        var publicKeys = new[]
        {
            rsa.CreateJwk("1", JsonWebKeyUseNames.Sig, false, KeyOperations.Sign, KeyOperations.Verify),
            rsa.CreateJwk("2", JsonWebKeyUseNames.Enc, false, KeyOperations.Encrypt, KeyOperations.Decrypt)
        };
        _privateKeySet = privateKeys.ToJwks();
        _privateKeySet.SkipUnresolvedJsonWebKeys = false;
        _publicKeySet = publicKeys.ToJwks();
    }

    public Task<JsonWebKeySet> GetPublicKeys(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_publicKeySet);
    }

    public Task<SigningCredentials> GetSigningKey(string alg, CancellationToken cancellationToken = default)
    {
        var signingKey = _privateKeySet.Keys.First(k => k.Use == JsonWebKeyUseNames.Sig && k.Alg == alg);

        return Task.FromResult(new SigningCredentials(signingKey, signingKey.Alg));
    }

    public Task<SecurityKey> GetEncryptionKey(string alg, CancellationToken cancellationToken = default)
    {
        var signingKey = _privateKeySet.GetEncryptionKeys().First();

        return Task.FromResult(signingKey);
    }

    public Task<SigningCredentials> GetDefaultSigningKey(CancellationToken cancellationToken = default)
    {
        var signingKey = _privateKeySet.Keys.First(k => k.Use == JsonWebKeyUseNames.Sig);

        return Task.FromResult(new SigningCredentials(signingKey, signingKey.Alg));
    }

    public Task<bool> Add(JsonWebKey key, CancellationToken cancellationToken = default)
    {
        if (key.HasPrivateKey)
        {
            _privateKeySet.Keys.Add(key);
        }
        else
        {
            _publicKeySet.Keys.Add(key);
        }

        return Task.FromResult(true);
    }

    public Task<bool> Rotate(JsonWebKeySet keySet, CancellationToken cancellationToken = default)
    {
        _publicKeySet.Keys.Clear();
        _privateKeySet.Keys.Clear();
        foreach (var key in keySet.Keys)
        {
            if (key.HasPrivateKey)
            {
                _privateKeySet.Keys.Add(key);
            }
            else
            {
                _publicKeySet.Keys.Add(key);
            }
        }

        return Task.FromResult(true);
    }
}