namespace SimpleIdentityServer.Core.Validators
{
    using System.Threading.Tasks;
    using Common.Models;
    using Parameters;

    public interface IAuthorizationCodeGrantTypeParameterAuthEdpValidator
    {
        Task<Client> ValidateAsync(AuthorizationParameter parameter);
    }
}