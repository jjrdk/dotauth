namespace SimpleIdentityServer.Scim.Core.Factories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SimpleIdentityServer.Core.Common.Models;

    public interface ICommonAttributesFactory
    {
        JProperty CreateIdJson(Representation representation);
        JProperty CreateIdJson(string id);
        Task<RepresentationAttribute> CreateId(Representation representation);
        IEnumerable<JProperty> CreateMetaDataAttributeJson(Representation representation, string location);
        Task<RepresentationAttribute> CreateMetaDataAttribute(Representation representation, string location);
        string GetFullPath(string key);
    }
}