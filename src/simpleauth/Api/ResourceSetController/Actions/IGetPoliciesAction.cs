namespace SimpleAuth.Api.ResourceSetController.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGetPoliciesAction
    {
        Task<IEnumerable<string>> Execute(string resourceId);
    }
}