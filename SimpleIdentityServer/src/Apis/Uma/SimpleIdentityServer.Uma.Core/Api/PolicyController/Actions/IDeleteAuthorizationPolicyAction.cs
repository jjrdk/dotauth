namespace SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions
{
    using System.Threading.Tasks;

    public interface IDeleteAuthorizationPolicyAction
    {
        Task<bool> Execute(string policyId);
    }
}