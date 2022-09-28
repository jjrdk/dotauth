namespace SimpleAuth.Api.Device;

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Errors;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Repositories;
using SimpleAuth.Shared.Requests;
using SimpleAuth.Shared.Responses;

/// <summary>
/// Defines the device authorization.
/// </summary>
public sealed class DeviceAuthorizationActions
{
    private readonly RuntimeSettings _settings;
    private readonly IDeviceAuthorizationStore _store;
    private readonly IClientStore _clientStore;
    private readonly ILogger _logger;
    private static readonly char[] Characters = "ABCDEFGHIJKMPRSTVXYZ0123456789".ToCharArray();
    private static readonly Random Rnd = new(DateTime.UtcNow.Millisecond);

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceAuthorizationActions"/> class.
    /// </summary>
    /// <param name="settings">The runtime settings.</param>
    /// <param name="store">The device authorization store.</param>
    /// <param name="clientStore">The client store.</param>
    /// <param name="logger">The logger.</param>
    public DeviceAuthorizationActions(RuntimeSettings settings, IDeviceAuthorizationStore store, IClientStore clientStore, ILogger logger)
    {
        _settings = settings;
        _store = store;
        _clientStore = clientStore;
        _logger = logger;
    }

    /// <summary>
    /// Starts the device authorization request.
    /// </summary>
    /// <param name="clientId">The client id of the requesting application.</param>
    /// <param name="authority">The token authority.</param>
    /// <param name="scopes">The requested scopes.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    public async Task<Option<DeviceAuthorizationData>> StartDeviceAuthorizationRequest(string clientId, Uri authority, string[] scopes, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientId)
            || await _clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false) == null)
        {
            _logger.LogError("Client not found: {clientId}", clientId);
            return new ErrorDetails
            {
                Detail = $"Client not found: {clientId}",
                Title = ErrorCodes.InvalidClient,
                Status = HttpStatusCode.BadRequest
            };
        }

        var userCode = GenerateUserCode();
        var response = new DeviceAuthorizationResponse
        {
            DeviceCode = Guid.NewGuid().ToString("N"),
            ExpiresIn = (int)_settings.DeviceAuthorizationLifetime.TotalSeconds,
            Interval = (int)_settings.DevicePollingInterval.TotalSeconds,
            UserCode = userCode,
            VerificationUri = $"{authority.AbsoluteUri}{CoreConstants.EndPoints.Device}",
            VerificationUriComplete = $"{authority.AbsoluteUri}{CoreConstants.EndPoints.Device}?user_code={userCode}"
        };
        var now = DateTimeOffset.UtcNow;
        var request = new DeviceAuthorizationData
        {
            ClientId = clientId,
            Approved = false,
            DeviceCode = response.DeviceCode,
            Expires = now.AddSeconds(response.ExpiresIn),
            Interval = response.Interval,
            Response = response,
            Scopes = scopes,
            LastPolled = now
        };
        var option = await _store.Save(request, cancellationToken).ConfigureAwait(false);
        return option switch
        {
            Option.Success => request,
            Option.Error e => e.Details,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string GenerateUserCode()
    {
        var buffer = Enumerable.Repeat(0, 8).Select(_ => Characters[Rnd.Next(Characters.Length)]).ToArray();
        return new string(buffer);
    }
}