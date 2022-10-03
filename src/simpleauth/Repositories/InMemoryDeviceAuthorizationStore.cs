namespace DotAuth.Repositories;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;

internal sealed class InMemoryDeviceAuthorizationStore : IDeviceAuthorizationStore
{
    private readonly List<DeviceAuthorizationData> _requests = new();

    /// <inheritdoc />
    public Task<Option<DeviceAuthorizationResponse>> Get(string userCode, CancellationToken cancellationToken = default)
    {
        var result = _requests.First(x => x.Response.UserCode == userCode);

        return Task.FromResult<Option<DeviceAuthorizationResponse>>(result.Response);
    }

    /// <inheritdoc />
    public Task<Option<DeviceAuthorizationData>> Get(string clientId, string deviceCode, CancellationToken cancellationToken = default)
    {
        var result = _requests.First(x => x.ClientId == clientId && x.DeviceCode == deviceCode);

        return Task.FromResult<Option<DeviceAuthorizationData>>(result);
    }

    /// <inheritdoc />
    public Task<Option> Approve(string userCode, CancellationToken cancellationToken = default)
    {
        var result = _requests.FirstOrDefault(x => x.Response.UserCode == userCode);
        if (result == null)
        {
            return Task.FromResult<Option>(
                new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = Strings.RequestIsNotValid,
                        Status = HttpStatusCode.BadRequest
                    }));
        }
        result.Approved = true;

        return Task.FromResult<Option>(new Option.Success());
    }

    /// <inheritdoc />
    public Task<Option> Save(DeviceAuthorizationData request, CancellationToken cancellationToken = default)
    {
        _requests.Add(request);
        return Task.FromResult<Option>(new Option.Success());
    }

    /// <inheritdoc />
    public Task<Option> Remove(DeviceAuthorizationData authRequest, CancellationToken cancellationToken)
    {
        _requests.Remove(authRequest);
        return Task.FromResult<Option>(new Option.Success());
    }
}