using SimpleIdentityServer.Scim.Mapping.Ad.Models;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Extensions
{
    internal static class CloneExtensions
    {
        public static AdMapping Copy(this AdMapping adMapping)
        {
            return new AdMapping
            {
                AdPropertyName = adMapping.AdPropertyName,
                AttributeId = adMapping.AttributeId,
                CreateDateTime = adMapping.CreateDateTime,
                SchemaId = adMapping.SchemaId,
                UpdateDateTime = adMapping.UpdateDateTime
            };
        }
    }
}
