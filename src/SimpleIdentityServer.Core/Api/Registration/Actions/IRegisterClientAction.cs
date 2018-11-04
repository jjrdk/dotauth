namespace SimpleIdentityServer.Core.Api.Registration.Actions
{
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Responses;

    public interface IRegisterClientAction
    {
        Task<ClientRegistrationResponse> Execute(RegistrationParameter registrationParameter);
    }
}