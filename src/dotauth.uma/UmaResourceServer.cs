namespace DotAuth.Uma;

using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using Shared.Responses;

public class UmaResourceServer : IUmaResourceServer
{
    private readonly IResourceStore _store;
    private readonly IDataStore _dataStore;

    public UmaResourceServer(
        IResourceStore store,
        IDataStore dataStore)
    {
        _store = store;
        _dataStore = dataStore;
    }

    public async Task<Option<ResourceResult>> GetResource(
        string resourceId,
        GrantedTokenResponse? principal,
        CancellationToken cancellationToken,
        params string[] scopes)
    {
        if (principal == null)
        {
            return new ErrorDetails { Detail = "No user", Status = HttpStatusCode.Unauthorized, Title = "No user" };
        }
        var registration = await _store.GetById(resourceId, cancellationToken).ConfigureAwait(false);
        
        if (registration == null)
        {
            return new Option<ResourceResult>.Error(
                new ErrorDetails { Title = "Not Found", Detail = "Not Found", Status = HttpStatusCode.NotFound });
        }
        if (!principal.GetUncheckedPrincipal().CheckResourceAccess(registration, scopes))
        {
            return new Option<ResourceResult>.Error(
                new ErrorDetails
                {
                    Detail = "Forbidden",
                    Title = "Forbidden",
                    Status = HttpStatusCode.Forbidden
                });
        }

        if (registration is not ResourceContent content)
        {
            return new ErrorDetails
            {
                Detail = "Invalid request",
                Title = "Invalid resource",
                Status = HttpStatusCode.BadRequest
            };
        }

        var fileStreamTasks = content.Files.Select(f => (f.MimeType, _dataStore.Get(f.Location, cancellationToken))).ToArray();
        var filestreams = await Task.WhenAll(fileStreamTasks.Select(x => x.Item2)).ConfigureAwait(false);
        if (filestreams.Any(s => s == null))
        {
            return new Option<ResourceResult>.Error(
                new ErrorDetails { Detail = "Not found", Status = HttpStatusCode.NotFound, Title = "Not found" });
        }

        var data = await Task.WhenAll(
            fileStreamTasks.Select(
                async t => new DownloadData
                {
                    Content = await t.Item2.ToByteArray().ConfigureAwait(false),
                    MimeType = t.MimeType
                })).ConfigureAwait(false);
        return new Option<ResourceResult>.Result(
            new ResourceDownload
            {
                Description = registration.Description,
                Name = registration.Name,
                Type = registration.Type,
                Data = data
            });
    }
}