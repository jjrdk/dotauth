namespace SimpleIdentityServer.Scim.Core.Apis
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Results;

    public interface IAddRepresentationAction
    {
        Task<ApiActionResult> Execute(JObject jObj, string locationPattern, string schemaId, string resourceType, string id = null);
    }
}