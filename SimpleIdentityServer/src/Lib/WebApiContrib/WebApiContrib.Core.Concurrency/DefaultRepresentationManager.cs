using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApiContrib.Core.Storage;

namespace WebApiContrib.Core.Concurrency
{
    internal sealed class DefaultRepresentationManager : IRepresentationManager
    {
        public Task AddOrUpdateRepresentationAsync(Controller controller, string representationId, bool addHeader = true)
        {
            throw new System.NotImplementedException();
        }

        public Task AddOrUpdateRepresentationAsync(Controller controller, string representationId, string etag, bool addHeader = true)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> CheckRepresentationExistsAsync(Controller controller, string representationId)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> CheckRepresentationHasChangedAsync(Controller controller, string representationId)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Record>> GetRepresentations()
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveRepresentations()
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateHeader(Controller controller, string representationId)
        {
            throw new System.NotImplementedException();
        }
    }
}
