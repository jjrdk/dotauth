namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Results;

    public interface IGetPolicyOperation
    {
        Task<GetPolicyResult> ExecuteAsync(string id, string url, string token);
    }
}