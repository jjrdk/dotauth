namespace SimpleIdentityServer.Core.Api.Registration.Actions
{
    using System.Threading.Tasks;
    using Common.DTOs.Responses;
    using Parameters;

    public interface IRegisterClientAction
    {
        Task<ClientRegistrationResponse> Execute(RegistrationParameter registrationParameter);
    }
}