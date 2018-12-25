namespace SimpleAuth.Api.Scopes.Actions
{
    using System.Threading.Tasks;

    public interface IDeleteScopeOperation
    {
        Task<bool> Execute(string scopeName);
    }
}