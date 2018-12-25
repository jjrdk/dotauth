namespace SimpleAuth.Api.Authorization
{
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IAuthorizationActions
    {
        Task<EndpointResult> GetAuthorization(AuthorizationParameter parameter, IPrincipal claimsPrincipal, string issuerName);
    }
}