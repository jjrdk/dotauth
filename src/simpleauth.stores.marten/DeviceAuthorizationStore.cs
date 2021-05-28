namespace SimpleAuth.Stores.Marten
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Marten;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public class MartenDeviceAuthorizationStore : IDeviceAuthorizationStore
    {
        private readonly Func<IDocumentSession> _sessionFunc;

        public MartenDeviceAuthorizationStore(Func<IDocumentSession> sessionFunc)
        {
            _sessionFunc = sessionFunc;
        }

        /// <inheritdoc />
        public async Task<Option<DeviceAuthorizationResponse>> Get(string userCode, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFunc();
            var request = await session.Query<DeviceAuthorizationRequest>()
                .Where(x => x.Response.UserCode == userCode)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            return request switch
            {
                null => new ErrorDetails
                {
                    Detail = ErrorMessages.NotFound,
                    Title = ErrorMessages.NotFound,
                    Status = HttpStatusCode.NotFound
                },
                _ => request!.Response
            };
        }

        /// <inheritdoc />
        public Task<Option> Approve(string userCode, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFunc();
            session.Patch<DeviceAuthorizationRequest>(x => x.Response.UserCode == userCode).Set(x => x.Approved, true);
            return Task.FromResult<Option>(new Option.Success());
        }

        /// <inheritdoc />
        public async Task<Option> Save(DeviceAuthorizationRequest request, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFunc();
            session.Store(request);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new Option.Success();
        }
    }
}