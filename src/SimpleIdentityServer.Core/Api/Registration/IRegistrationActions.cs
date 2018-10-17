namespace SimpleIdentityServer.Core.Api.Registration
{
    using System.Threading.Tasks;
    using Common.DTOs.Responses;
    using Parameters;

    public interface IRegistrationActions
    {
        Task<ClientRegistrationResponse> PostRegistration(RegistrationParameter registrationParameter);
    }
}