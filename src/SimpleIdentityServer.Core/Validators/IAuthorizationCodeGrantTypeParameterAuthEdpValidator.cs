namespace SimpleAuth.Validators
{
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    public interface IAuthorizationCodeGrantTypeParameterAuthEdpValidator
    {
        Task<Client> ValidateAsync(AuthorizationParameter parameter);
    }
}