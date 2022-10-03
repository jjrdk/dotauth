namespace DotAuth.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;

internal sealed class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
{
    private static readonly Dictionary<string, AuthorizationCode> MappingStringToAuthCodes;

    static InMemoryAuthorizationCodeStore()
    {
        MappingStringToAuthCodes = new Dictionary<string, AuthorizationCode>();
    }

    public Task<AuthorizationCode?> Get(string code, CancellationToken cancellationToken)
    {
        return !MappingStringToAuthCodes.ContainsKey(code)
            ? Task.FromResult<AuthorizationCode?>(null)
            : Task.FromResult<AuthorizationCode?>(MappingStringToAuthCodes[code]);
    }

    public Task<bool> Add(AuthorizationCode authorizationCode, CancellationToken cancellationToken)
    {
        if (authorizationCode == null)
        {
            throw new ArgumentNullException(nameof(authorizationCode));
        }

        if (MappingStringToAuthCodes.ContainsKey(authorizationCode.Code))
        {
            return Task.FromResult(false);
        }

        MappingStringToAuthCodes.Add(authorizationCode.Code, authorizationCode);
        return Task.FromResult(true);
    }

    public Task<bool> Remove(string authorizationCode, CancellationToken cancellationToken)
    {
        if (authorizationCode == null)
        {
            throw new ArgumentNullException(nameof(authorizationCode));
        }

        if (!MappingStringToAuthCodes.ContainsKey(authorizationCode))
        {
            return Task.FromResult(false);
        }

        MappingStringToAuthCodes.Remove(authorizationCode);
        return Task.FromResult(true);
    }
}