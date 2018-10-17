namespace SimpleIdentityServer.Scim.Core.Apis
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Results;

    public interface IBulkAction
    {
        Task<ApiActionResult> Execute(JObject jObj, string baseUrl);
    }
}