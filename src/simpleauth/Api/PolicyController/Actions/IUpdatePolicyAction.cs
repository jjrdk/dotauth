namespace SimpleAuth.Api.PolicyController.Actions
{
    using System.Threading.Tasks;
    using Parameters;

    public interface IUpdatePolicyAction
    {
        Task<bool> Execute(UpdatePolicyParameter updatePolicyParameter);
    }
}