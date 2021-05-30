namespace SimpleAuth.Api.Device
{
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

    public class DeviceAuthorizationActions
    {
        private readonly RuntimeSettings _settings;
        private readonly IDeviceAuthorizationStore _store;
        private readonly IClientStore _clientStore;
        private readonly ILogger _logger;
        private static readonly char[] Characters = "ABCDEFGHIJKMOPRSTVXYZ123456789".ToCharArray();
        private static readonly Random Rnd = new(DateTime.UtcNow.Millisecond);

        public DeviceAuthorizationActions(RuntimeSettings settings, IDeviceAuthorizationStore store, IClientStore clientStore, ILogger logger)
        {
            _settings = settings;
            _store = store;
            _clientStore = clientStore;
            _logger = logger;
        }

        public async Task<Option<DeviceAuthorizationData>> StartDeviceAuthorizationRequest(string clientId, Uri authority, string[] scopes, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(clientId)
                || await _clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false) == null)
            {
                var format = "Client not found: {0}";
                _logger.LogError(format, clientId);
                return new ErrorDetails
                {
                    Detail = string.Format(format, clientId),
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
}
