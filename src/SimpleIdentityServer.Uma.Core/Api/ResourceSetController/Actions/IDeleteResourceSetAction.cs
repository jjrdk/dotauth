namespace SimpleAuth.Uma.Api.ResourceSetController.Actions
{
    using System.Threading.Tasks;

    internal interface IDeleteResourceSetAction
    {
        Task<bool> Execute(string resourceSetId);
    }
}