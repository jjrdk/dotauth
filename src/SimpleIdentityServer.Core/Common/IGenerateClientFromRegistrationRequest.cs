namespace SimpleIdentityServer.Core.Common
{
    using Core.Parameters;
    using Shared.Models;

    public interface IGenerateClientFromRegistrationRequest
    {
        Client Execute(RegistrationParameter registrationParameter);
    }
}