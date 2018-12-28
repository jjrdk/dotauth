namespace SimpleAuth.Uma.Api.ResourceSetController.Actions
{
    using System.Threading.Tasks;
    using Parameters;

    internal interface IUpdateResourceSetAction
    {
        Task<bool> Execute(UpdateResourceSetParameter udpateResourceSetParameter);
    }
}