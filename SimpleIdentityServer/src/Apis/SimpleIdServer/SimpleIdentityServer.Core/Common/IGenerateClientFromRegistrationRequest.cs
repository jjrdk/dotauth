namespace SimpleIdentityServer.Core.Common
{
    using Core.Parameters;
    using Models;

    public interface IGenerateClientFromRegistrationRequest
    {
        Client Execute(RegistrationParameter registrationParameter);
    }
}