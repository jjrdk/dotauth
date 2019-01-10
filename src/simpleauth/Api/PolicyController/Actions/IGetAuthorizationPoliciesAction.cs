namespace SimpleAuth.Api.PolicyController.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGetAuthorizationPoliciesAction
    {
        Task<ICollection<string>> Execute();
    }
}