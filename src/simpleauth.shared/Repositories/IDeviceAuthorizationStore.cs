namespace SimpleAuth.Shared.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public interface IDeviceAuthorizationStore
    {
        public Task<Option<DeviceAuthorizationResponse>> Get(string userCode, CancellationToken cancellationToken = default);

        public Task<Option<DeviceAuthorizationData>> Get(string clientId, string deviceCode, CancellationToken cancellationToken = default);

        public Task<Option> Approve(string userCode, CancellationToken cancellationToken = default);

        Task<Option> Save(DeviceAuthorizationData request, CancellationToken cancellationToken = default);
    }
}