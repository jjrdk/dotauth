namespace SimpleAuth.Uma.Api.ResourceSetController.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal interface IGetAllResourceSetAction
    {
        Task<IEnumerable<string>> Execute();
    }
}