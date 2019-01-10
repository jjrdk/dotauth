namespace SimpleAuth.Api.PolicyController.Actions
{
    using System.Threading.Tasks;

    public interface IDeleteAuthorizationPolicyAction
    {
        Task<bool> Execute(string policyId);
    }
}