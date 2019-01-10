namespace SimpleAuth.Api.PolicyController.Actions
{
    using System.Threading.Tasks;

    public interface IDeleteResourcePolicyAction
    {
        Task<bool> Execute(string id, string resourceId);
    }
}