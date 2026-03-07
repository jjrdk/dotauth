namespace DotAuth.Repositories;

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
        return !MappingStringToAuthCodes.TryGetValue(code, out var authCode)
            ? Task.FromResult<AuthorizationCode?>(null)
            : Task.FromResult<AuthorizationCode?>(authCode);
    }

    public Task<bool> Add(AuthorizationCode authorizationCode, CancellationToken cancellationToken)
    {
        return Task.FromResult(MappingStringToAuthCodes.TryAdd(authorizationCode.Code, authorizationCode));
    }

    public Task<bool> Remove(string authorizationCode, CancellationToken cancellationToken)
    {
        return Task.FromResult(MappingStringToAuthCodes.Remove(authorizationCode));
    }
}
