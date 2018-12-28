namespace SimpleAuth.Uma.Api.ResourceSetController.Actions
{
    using System.Threading.Tasks;
    using Models;

    internal interface IGetResourceSetAction
    {
        Task<ResourceSet> Execute(string id);
    }
}