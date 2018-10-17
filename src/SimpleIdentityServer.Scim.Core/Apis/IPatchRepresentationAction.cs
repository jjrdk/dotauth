namespace SimpleIdentityServer.Scim.Core.Apis
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Results;

    public interface IPatchRepresentationAction
    {
        Task<ApiActionResult> Execute(string id, JObject jObj, string schemaId, string locationPattern);
    }
}