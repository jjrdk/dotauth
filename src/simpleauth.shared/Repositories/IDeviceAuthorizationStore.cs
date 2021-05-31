namespace SimpleAuth.Shared.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the device authorization store interface.
    /// </summary>
    public interface IDeviceAuthorizationStore
    {
        /// <summary>
        /// Gets the <see cref="DeviceAuthorizationResponse"/> for the user code.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>A <see cref="DeviceAuthorizationResponse"/> as an async operation.</returns>
        public Task<Option<DeviceAuthorizationResponse>> Get(string userCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="DeviceAuthorizationData"/> for the device code.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="deviceCode">The device code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>A <see cref="DeviceAuthorizationData"/> as an async operation.</returns>
        public Task<Option<DeviceAuthorizationData>> Get(string clientId, string deviceCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Approves the authorization request.
        /// </summary>
        /// <param name="userCode">The user code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>An <see cref="Option"/> as an async operation.</returns>
        public Task<Option> Approve(string userCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the passed <see cref="DeviceAuthorizationData"/>.
        /// </summary>
        /// <param name="request">The <see cref="DeviceAuthorizationData"/> to save.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>An <see cref="Option"/> as an async operation.</returns>
        Task<Option> Save(DeviceAuthorizationData request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the passed <see cref="DeviceAuthorizationData"/>.
        /// </summary>
        /// <param name="authRequest">The <see cref="DeviceAuthorizationData"/> to save.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>An <see cref="Option"/> as an async operation.</returns>
        Task<Option> Remove(DeviceAuthorizationData authRequest, CancellationToken cancellationToken);
    }
}