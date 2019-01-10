namespace SimpleAuth.Api.PolicyController.Actions
{
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IGetAuthorizationPolicyAction
    {
        Task<Policy> Execute(string policyId);
    }
}