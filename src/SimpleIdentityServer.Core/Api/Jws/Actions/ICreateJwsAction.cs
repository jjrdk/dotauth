namespace SimpleIdentityServer.Core.Api.Jws.Actions
{
    using System.Threading.Tasks;
    using Parameters;

    public interface ICreateJwsAction
    {
        Task<string> Execute(CreateJwsParameter createJwsParameter);
    }
}