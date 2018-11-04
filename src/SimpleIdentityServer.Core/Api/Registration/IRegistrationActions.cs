namespace SimpleIdentityServer.Core.Api.Registration
{
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Responses;

    public interface IRegistrationActions
    {
        Task<ClientRegistrationResponse> PostRegistration(RegistrationParameter registrationParameter);
    }
}