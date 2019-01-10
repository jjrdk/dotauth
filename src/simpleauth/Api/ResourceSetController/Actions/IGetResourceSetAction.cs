namespace SimpleAuth.Api.ResourceSetController.Actions
{
    using System.Threading.Tasks;
    using Shared.Models;

    internal interface IGetResourceSetAction
    {
        Task<ResourceSet> Execute(string id);
    }
}