namespace SimpleIdentityServer.Core.Api.Jws
{
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IJwsActions
    {
        Task<JwsInformationResult> GetJwsInformation(GetJwsParameter getJwsParameter);
        Task<string> CreateJws(CreateJwsParameter createJwsParameter);
    }
}