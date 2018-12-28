namespace SimpleAuth.Uma.Api.PolicyController.Actions
{
    using System.Threading.Tasks;
    using Parameters;

    public interface IUpdatePolicyAction
    {
        Task<bool> Execute(UpdatePolicyParameter updatePolicyParameter);
    }
}