using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;
using WebApiContrib.Core.Storage;

namespace SimpleIdentityServer.Uma.Host.Services
{
    public class DefaultRepresentationManager : IRepresentationManager
    {
        public Task AddOrUpdateRepresentationAsync(Controller controller, string representationId, bool addHeader = true)
        {
            return Task.FromResult(0);
        }

        public Task AddOrUpdateRepresentationAsync(Controller controller, string representationId, string etag, bool addHeader = true)
        {
            return Task.FromResult(0);
        }

        public Task<bool> CheckRepresentationExistsAsync(Controller controller, string representationId)
        {
            return Task.FromResult(false);
        }

        public Task<bool> CheckRepresentationHasChangedAsync(Controller controller, string representationId)
        {
            return Task.FromResult(false);
        }

        public Task<IEnumerable<Record>> GetRepresentations()
        {
            return Task.FromResult((IEnumerable<Record>)null);
        }

        public Task RemoveRepresentations()
        {
            return Task.FromResult(0);
        }

        public Task UpdateHeader(Controller controller, string representationId)
        {
            return Task.FromResult(0);
        }
    }
}
