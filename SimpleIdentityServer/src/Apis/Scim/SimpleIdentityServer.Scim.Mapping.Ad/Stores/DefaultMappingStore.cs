using SimpleIdentityServer.Scim.Mapping.Ad.Extensions;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Stores
{
    internal sealed class DefaultMappingStore : IMappingStore
    {
        private List<AdMapping> _adMappings;

        public DefaultMappingStore(List<AdMapping> adMappings)
        {
            _adMappings = adMappings == null ? new List<AdMapping>() : adMappings;
        }

        public Task<bool> AddMapping(AdMapping adMapping)
        {
            if (adMapping == null)
            {
                throw new ArgumentNullException(nameof(adMapping));
            }

            adMapping.CreateDateTime = DateTime.UtcNow;
            _adMappings.Add(adMapping.Copy());
            return Task.FromResult(true);
        }

        public Task<List<AdMapping>> GetAll()
        {
            return Task.FromResult(_adMappings);
        }

        public Task<AdMapping> GetMapping(string attributeId)
        {
            if (string.IsNullOrWhiteSpace(attributeId))
            {
                throw new ArgumentNullException(nameof(attributeId));
            }

            var mapping = _adMappings.FirstOrDefault(m => m.AttributeId == attributeId);
            if (mapping == null)
            {
                return Task.FromResult((AdMapping)null);
            }

            return Task.FromResult(mapping.Copy());
        }

        public Task<bool> Remove(string attributeId)
        {
            if (string.IsNullOrWhiteSpace(attributeId))
            {
                throw new ArgumentNullException(nameof(attributeId));
            }

            var mapping = _adMappings.FirstOrDefault(m => m.AttributeId == attributeId);
            if (mapping == null)
            {
                return Task.FromResult(false);
            }

            _adMappings.Remove(mapping);
            return Task.FromResult(true);
        }
    }
}
