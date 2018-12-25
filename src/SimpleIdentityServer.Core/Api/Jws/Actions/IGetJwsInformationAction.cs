namespace SimpleAuth.Api.Jws.Actions
{
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IGetJwsInformationAction
    {
        Task<JwsInformationResult> Execute(GetJwsParameter getJwsParameter);
    }
}