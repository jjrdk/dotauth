namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Results;

    public interface IGetPoliciesOperation
    {
        Task<GetPoliciesResult> ExecuteAsync(string url, string token);
    }
}