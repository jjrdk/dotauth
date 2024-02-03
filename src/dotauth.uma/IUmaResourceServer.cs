namespace DotAuth.Uma;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using Shared.Responses;

public interface IUmaResourceServer
{
    Task<Option<ResourceResult>> GetResource(
        string resourceId,
        GrantedTokenResponse? principal,
        CancellationToken cancellationToken,
        params string[] scopes);
}