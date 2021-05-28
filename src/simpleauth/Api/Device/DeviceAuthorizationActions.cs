namespace SimpleAuth.Api.Device
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public class DeviceAuthorizationActions
    {
        private readonly IDeviceAuthorizationStore _store;
        private static readonly char[] Characters = "ABCDEFGHIJKMOPRSTVXYZ123456789".ToCharArray();
        private static readonly Random Rnd = new(DateTime.UtcNow.Millisecond);

        public DeviceAuthorizationActions(IDeviceAuthorizationStore store)
        {
            _store = store;
        }

        public async Task<Option<DeviceAuthorizationRequest>> StartDeviceAuthorizationRequest(string clientId, Uri authority, string[] scopes)
        {
            var userCode = GenerateUserCode();
            var response = new DeviceAuthorizationResponse
            {
                DeviceCode = Guid.NewGuid().ToString("N"),
                ExpiresIn = 1800,
                Interval = 5,
                UserCode = userCode,
                VerificationUri = $"{authority.AbsoluteUri}{CoreConstants.EndPoints.Device}",
                VerificationUriComplete = $"{authority.AbsoluteUri}{CoreConstants.EndPoints.Device}?user_code={userCode}"
            };
            var request = new DeviceAuthorizationRequest
            {
                ClientId = clientId,
                Approved = false,
                DeviceCode = response.DeviceCode,
                Expires = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn),
                Interval = response.Interval,
                Response = response,
                Scope = scopes
            };
            var option = await _store.Save(request).ConfigureAwait(false);
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
