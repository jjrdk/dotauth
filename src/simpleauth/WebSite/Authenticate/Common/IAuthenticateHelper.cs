namespace SimpleAuth.WebSite.Authenticate.Common
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IAuthenticateHelper
    {
        Task<EndpointResult> ProcessRedirection(
            AuthorizationParameter authorizationParameter,
            string code,
            string subject,
            List<Claim> claims, string issuerName);
    }
}