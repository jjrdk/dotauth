namespace SimpleAuth.Uma.Api.ResourceSetController.Actions
{
    using System.Threading.Tasks;
    using Parameters;

    internal interface IAddResourceSetAction
    {
        Task<string> Execute(AddResouceSetParameter addResourceSetParameter);
    }
}