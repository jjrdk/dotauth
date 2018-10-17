namespace SimpleIdentityServer.Scim.Core.Apis
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Results;

    public interface IUpdateRepresentationAction
    {
        Task<ApiActionResult> Execute(string id, JObject jObj, string schemaId, string locationPattern, string resourceType);
    }
}