namespace SimpleIdentityServer.Core.Validators
{
    using System.Threading.Tasks;
    using Parameters;
    using SimpleAuth.Shared.Models;

    public interface IAuthorizationCodeGrantTypeParameterAuthEdpValidator
    {
        Task<Client> ValidateAsync(AuthorizationParameter parameter);
    }
}