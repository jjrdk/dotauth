namespace SimpleAuth.Api.Token.Actions
{
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Parameters;

    public interface IRevokeTokenAction
    {
        Task<bool> Execute(RevokeTokenParameter revokeTokenParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
    }
}